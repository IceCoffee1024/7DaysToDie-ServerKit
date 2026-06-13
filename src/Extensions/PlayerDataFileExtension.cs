using SdtdServerKit.Models;

namespace SdtdServerKit.Extensions
{
    /// <summary>
    /// Extension methods for PlayerDataFile.
    /// </summary>
    internal static class PlayerDataFileExtension
    {
        /// <summary>
        /// Gets the total stack count of a specified item in the player's inventory.
        /// </summary>
        /// <param name="pdf">The PlayerDataFile instance.</param>
        /// <param name="itemName">The name of the item.</param>
        /// <param name="language"></param>
        /// <returns>The total stack count of the item.</returns>
        public static int GetInventoryStackCount(this PlayerDataFile pdf, string itemName, Language language)
        {
            int count = 0;
            var bag = ProcessInv(pdf.bag, pdf.id, language);
            var belt = ProcessInv(pdf.inventory, pdf.id, language);

            foreach (var item in bag)
            {
                if (item.ItemName == itemName)
                {
                    count += item.Count;
                }
            }

            foreach (var item in belt)
            {
                if (item.ItemName == itemName)
                {
                    count += item.Count;
                }
            }

            return count;
        }

        #region Get Inventory

        /// <summary>
        /// Gets the player's inventory.
        /// </summary>
        /// <param name="pdf">The PlayerDataFile instance.</param>
        /// <param name="language"></param>
        /// <returns>The player's inventory.</returns>
        public static Models.Inventory GetInventory(this PlayerDataFile pdf, Language language)
        {
            try
            {
                return new Models.Inventory()
                {
                    Bag = ProcessInv(pdf.bag, pdf.id, language),
                    Belt = ProcessInv(pdf.inventory, pdf.id, language),
                    Equipment = ProcessEqu(pdf.equipment, pdf.id, language)
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Get player inventory from PlayerDataFile failed.", ex);
            }
        }

        private static IEnumerable<InvItem> ProcessInv(ItemStack[] sourceFields, int entityId, Language language)
        {
            var target = new List<InvItem>(sourceFields.Length);

            foreach (var field in sourceFields)
            {
                var invItem = CreateInvItem(field.itemValue, field.count, entityId, language);
                if (invItem != null && field.itemValue.Modifications != null)
                {
                    ProcessParts(field.itemValue.Modifications, invItem, entityId, language);
                    target.Add(invItem);
                }
            }

            return target;
        }

        private static IEnumerable<InvItem> ProcessEqu(Equipment sourceEquipment, int entityId, Language language)
        {
            //int slotCount = sourceEquipment.GetSlotCount();
            //var equipment = new InvItem?[slotCount];
            //for (int i = 0; i < slotCount; i++)
            //{
            //    equipment[i] = CreateInvItem(sourceEquipment.GetSlotItem(i), 1, entityId);
            //}

            int slotCount = sourceEquipment.GetSlotCount();
            var target = new List<InvItem>(slotCount);
            for (int i = 0; i < slotCount; i++)
            {
                var itemValue = sourceEquipment.GetSlotItem(i);
                var invItem = CreateInvItem(itemValue, 1, entityId, language);
                if (invItem != null)
                {
                    ProcessParts(itemValue.Modifications, invItem, entityId, language);
                    target.Add(invItem);
                }
            }

            return target;
        }

        private static void ProcessParts(ItemValue[] parts, InvItem item, int entityId, Language language)
        {
            int length = parts.Length;

            InvItem?[] itemParts = new InvItem[length];

            for (int i = 0; i < length; i++)
            {
                var partItem = CreateInvItem(parts[i], 1, entityId, language);
                if (partItem != null && parts[i].Modifications != null)
                {
                    ProcessParts(parts[i].Modifications, partItem, entityId, language);
                }

                itemParts[i] = partItem;
            }

            item.Parts = itemParts;
        }

        private static InvItem? CreateInvItem(ItemValue itemValue, int count, int entityId, Language language)
        {
            try
            {
                if (count <= 0 || itemValue == null || itemValue.Equals(ItemValue.None))
                {
                    return null;
                }

                var itemClass = ItemClass.list[itemValue.type];

                if (itemClass == null)
                {
                    return null;
                }

                //int maxAllowed = itemClass.Stacknumber.Value;
                string name = itemClass.GetItemName();

                //string steamId = ConnectionManager.Instance.Clients.ForEntityId(entityId).playerId;

                //var inventoryCheck = FunctionManager.AntiCheat.InventoryCheck;
                //if (inventoryCheck.IsEnabled)
                //{
                //    inventoryCheck.Execute(steamId, name, count, maxAllowed);
                //}

                int quality = 0;
                string? qualityColor = null;
                if (itemValue.HasQuality)
                {
                    quality = itemValue.Quality;
                    qualityColor = QualityInfo.GetQualityColorHex(quality);
                }
                else
                {
                    quality = -1;
                }
                
                InvItem item = new InvItem()
                {
                    ItemName = name,
                    LocalizationName = Utilities.Utils.GetLocalization(name, language),
                    Count = count,
                    MaxStackAllowed = itemClass.Stacknumber.Value,
                    Quality = quality,
                    QualityColor = qualityColor,
                    UseTimes = itemValue.UseTimes,
                    MaxUseTimes = itemValue.MaxUseTimes,
                    IsMod = itemValue.IsMod,
                    Seed = itemValue.Seed,
                };

                return item;
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "Error in PlayerDataFileExtension.CreateInvItem" + Environment.NewLine + JsonConvert.SerializeObject(itemValue));
                return null;
            }
        }

        #endregion Get Inventory
    }
}