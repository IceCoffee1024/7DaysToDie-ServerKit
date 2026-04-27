using GameEvent.SequenceActions;
using Noemax.GZip;
using System.Collections;
using System.Reflection;
using System.Xml.Linq;
using System.Xml;
using static WorldStaticData;

namespace SdtdServerKit.Hooks
{
    internal static class WorldStaticDataHook
    {
        public const string TagPrefix = "ty_";
        public const string ActionPrefix = "action_";

        /// <summary>
        /// ToImplRemovePlayerItems
        /// </summary>
        public static void ReplaceXmls()
        {
            var xmlsToLoad = WorldStaticData.xmlsToLoad;
            var addedTags = new HashSet<string>();

            Type type = typeof(XmlLoadInfo);
            FieldInfo dataField = type.GetField("CompressedXmlData", BindingFlags.Instance | BindingFlags.Public);
            foreach (var item in xmlsToLoad)
            {
                string xmlName = item.XmlName;
                byte[] compressedXmlData = item.CompressedXmlData;
                if (xmlName == "blocks")
                {
                    var modified = AttachTags(xmlName, compressedXmlData, addedTags);
                    dataField.SetValue(item, modified);
                    CustomLogger.Info("Successfully attach blocks tags, length: " + modified.Length);
                }
                else if (xmlName == "items")
                {
                    var modified = AttachTags(xmlName, compressedXmlData, addedTags);
                    dataField.SetValue(item, modified);
                    CustomLogger.Info("Successfully attach items tags, length: " + modified.Length);
                }
                else if (xmlName == "gameevents")
                {
                    var modified = GenerateGameevents(xmlName, compressedXmlData, addedTags);
                    dataField.SetValue(item, modified);
                    CustomLogger.Info("Successfully generate gameevents, length: " + modified.Length);
                    break;
                }
            }
        }

        private static XmlDocument Decompresse(byte[] compressedXmlData)
        {
            using var memoryStream = new MemoryStream(compressedXmlData);
            using var deflateInputStream = new DeflateInputStream(memoryStream);
            using var decompressedMemoryStream = new MemoryStream();

            StreamUtils.StreamCopy(deflateInputStream, decompressedMemoryStream, null, true);

            var xmlDocument = new XmlDocument();
            decompressedMemoryStream.Seek(0, SeekOrigin.Begin);
            xmlDocument.Load(decompressedMemoryStream);

            return xmlDocument;
        }

        private static byte[] Compresse(XmlDocument xmlDocument)
        {
            using var compressedMemoryStream = new MemoryStream();
            using var deflateOutputStream = new DeflateOutputStream(compressedMemoryStream, 9);
            xmlDocument.Save(deflateOutputStream);
            return compressedMemoryStream.ToArray();
        }

        private static string[]? GetParentNodeTags(XmlNode currentNode, XmlNode rootNode, string xpathName)
        {
            var extendsNode = currentNode.SelectSingleNode("property[@name='Extends']") as XmlElement;
            if (extendsNode == null)
            {
                return null;
            }

            string extendsName = extendsNode.GetAttribute("value");
            var parentNode = rootNode.SelectSingleNode($"{xpathName}[@name='{extendsName}']");
            var parentTagsNode = parentNode.SelectSingleNode("property[@name='Tags']") as XmlElement;
            if (parentTagsNode != null)
            {
                return parentTagsNode.GetAttribute("value").Split(',');
            }
            else
            {
                return GetParentNodeTags(parentNode, rootNode, xpathName);
            }
        }

        private static byte[] AttachTags(string xmlName, byte[] compressedXmlData, HashSet<string> addedTags)
        {
            string xmlNameWithoutS = xmlName.Substring(0, xmlName.Length - 1);
            var xmlDocument = Decompresse(compressedXmlData);
            var rootNode = xmlDocument.SelectSingleNode(xmlName);
            var xmlNodeList = rootNode.ChildNodes;

            foreach (XmlElement item in xmlNodeList.OfType<XmlElement>())
            {
                var xmlElement = item;
                string itemName = xmlElement.GetAttribute("name");
                string tag = TagPrefix + itemName;

                var tagsNode = item.SelectSingleNode("property[@name='Tags']") as XmlElement;
                if (tagsNode == null)
                {
                    var newElement = xmlDocument.CreateElement("property");
                    newElement.SetAttribute("name", "Tags");

                    string[]? parentTags = GetParentNodeTags(item, rootNode, xmlNameWithoutS);
                    if (parentTags != null)
                    {
                        var _tags = parentTags.ToList();
                        _tags.Add(tag);
                        string newTags = string.Join(",", _tags);
                        newElement.SetAttribute("value", newTags);
                    }
                    else
                    {
                        newElement.SetAttribute("value", tag);
                    }

                    item.AppendChild(newElement);
                }
                else
                {
                    string[] tags = tagsNode.GetAttribute("value").Split(',');
                    if (tags.Contains(tag) == false)
                    {
                        var _tags = tags.ToList();
                        _tags.Add(tag);
                        string newTags = string.Join(",", _tags);
                        tagsNode.SetAttribute("value", newTags);
                    }
                }

                addedTags.Add(tag);
            }

            return Compresse(xmlDocument);
        }

        private static void ParseGameEventSequenceAction(XElement actionEl, GameEventActionSequence owner, BaseAction baseAction)
        {
            var dynamicProperties = new DynamicProperties();
            foreach (XElement item in actionEl.Elements("property"))
            {
                dynamicProperties.Add(item, true);
            }

            baseAction.Owner = owner;
            baseAction.ParseProperties(dynamicProperties);
            baseAction.Init();

            owner.Actions.Add(baseAction);
        }

        private static byte[] GenerateGameevents(string xmlName, byte[] compressedXmlData, HashSet<string> addedTags)
        {
            var xmlDocument = Decompresse(compressedXmlData);
            var rootNode = xmlDocument.SelectSingleNode(xmlName);

            foreach (var tag in addedTags)
            {
                string actionSequenceName = ActionPrefix + tag;
                var gameEventActionSequence = new GameEventActionSequence();

                var actionSequenceEl = xmlDocument.CreateElement("action_sequence");
                actionSequenceEl.SetAttribute("name", actionSequenceName);

                {
                    var actionEl = xmlDocument.CreateElement("action");
                    actionEl.SetAttribute("class", "RemoveItems");

                    var propertyEl = xmlDocument.CreateElement("property");
                    propertyEl.SetAttribute("name", "items_location");
                    propertyEl.SetAttribute("value", "Toolbelt,Backpack");// Equipment, Held
                    actionEl.AppendChild(propertyEl);

                    propertyEl = xmlDocument.CreateElement("property");
                    propertyEl.SetAttribute("name", "items_tags");
                    propertyEl.SetAttribute("value", tag);
                    actionEl.AppendChild(propertyEl);

                    actionSequenceEl.AppendChild(actionEl);

                    var action = new ActionRemoveItems();
                    ParseGameEventSequenceAction(actionEl.ToXElement(), gameEventActionSequence, action);
                }

                {
                    var actionEl = xmlDocument.CreateElement("action");
                    actionEl.SetAttribute("class", "PlaySound");

                    var propertyEl = xmlDocument.CreateElement("property");
                    propertyEl.SetAttribute("name", "sound");
                    propertyEl.SetAttribute("value", "ui_trader_purchase");
                    actionEl.AppendChild(propertyEl);

                    propertyEl = xmlDocument.CreateElement("property");
                    propertyEl.SetAttribute("name", "inside_head");
                    propertyEl.SetAttribute("value", "true");
                    actionEl.AppendChild(propertyEl);

                    actionSequenceEl.AppendChild(actionEl);

                    var action = new ActionPlaySound();
                    ParseGameEventSequenceAction(actionEl.ToXElement(), gameEventActionSequence, action);
                }

                rootNode.AppendChild(actionSequenceEl);

                gameEventActionSequence.Name = actionSequenceName;
                gameEventActionSequence.Init();
                GameEventManager.Current.AddSequence(gameEventActionSequence);
            }

            return Compresse(xmlDocument);
        }
    }
}
