using Namotion.Reflection;
using Newtonsoft.Json.Linq;
using SdtdServerKit.Constants;
using SdtdServerKit.Managers;
using SdtdServerKit.ServerSettings;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SystemInformation;
using UnityEngine;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// Server
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Server")]
    public class ServerController : ApiController
    {
        /// <summary>
        /// Execute console command.
        /// </summary>
        /// <param name="command">Command</param>
        /// <param name="inMainThread">Execute console command in main thread</param>
        /// <returns></returns>
        [HttpPost]
        [Route(nameof(ExecuteConsoleCommand))]
        public IEnumerable<string> ExecuteConsoleCommand([FromUri] string command, [FromUri] bool inMainThread = false)
        {
            return Utilities.Utils.ExecuteConsoleCommand(command, inMainThread);
        }

        /// <summary>
        /// Get allowed commands.
        /// </summary>
        [HttpGet]
        [Route(nameof(AllowedCommands))]
        public IEnumerable<AllowedCommand> AllowedCommands()
        {
            var allowedCommands = new List<AllowedCommand>();
            foreach (var consoleCommand in SdtdConsole.Instance.GetCommands())
            {
                var commands = consoleCommand.GetCommands();
                int commandPermissionLevel = GameManager.Instance.adminTools.Commands.GetCommandPermissionLevel(commands);

                allowedCommands.Add(new AllowedCommand()
                {
                    Commands = commands,
                    PermissionLevel = commandPermissionLevel,
                    Description = consoleCommand.GetDescription(),
                    Help = consoleCommand.GetHelp(),
                });
            }

            return allowedCommands;
        }

        /// <summary>
        /// Get allowed commands.
        /// </summary>
        [HttpGet]
        [Route(nameof(AllowSpawnedEntities))]
        public IEnumerable<AllowSpawnedEntity> AllowSpawnedEntities()
        {
            var result = new List<AllowSpawnedEntity>();
            int num = 1;
            foreach (var item in EntityClass.list.Dict.Values)
            {
                if (item.userSpawnType == EntityClass.UserSpawnType.Menu)
                {
                    result.Add(new AllowSpawnedEntity()
                    {
                        Id = num,
                        Name = item.entityClassName
                    });

                    ++num;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取丧尸本地化名称
        /// </summary>
        [HttpGet]
        [Route(nameof(ZombieEntityClasses))]
        public IEnumerable<object> ZombieEntityClasses([FromUri] Language language = Language.English)
        {
            var result = new List<(string className, string locName)>();
            foreach (var item in EntityClass.list.Dict.Values)
            {
                if (item.classname == null || string.IsNullOrEmpty(item.entityClassName))
                    continue;

                Type type = item.classname;
                bool matched = false;
                while (type != null && type != typeof(object))
                {
                    if (type.Name == "EntityZombie" || type.Name == "EntityEnemy"
                        || type.Name == "EntityAnimal" || type.Name == "EntityEnemyAnimal")
                    {
                        matched = true;
                        break;
                    }
                    type = type.BaseType;
                }

                if (matched)
                {
                    string locName = Utilities.Utils.GetLocalization(item.entityClassName, language);
                    result.Add((item.entityClassName, locName));
                }
            }

            result.Sort((a, b) => string.Compare(a.className, b.className, StringComparison.OrdinalIgnoreCase));

            return result.Select(r => new
            {
                entityClassName = r.className,
                localizationName = string.IsNullOrEmpty(r.locName) || r.locName == r.className ? null : r.locName,
            });
        }

        /// <summary>
        /// Get stats.
        /// </summary>
        [HttpGet]
        [Route(nameof(Stats))]
        public Stats Stats()
        {
            var gameManager = GameManager.Instance;
            var world = GameManager.Instance.World;
            var worldTime = world.GetWorldTime();
            var entityList = world.Entities.list;

            int zombies = 0;
            int animals = 0;
            foreach (var entity in entityList)
            {
                if (entity.IsAlive())
                {
                    if (entity is EntityEnemy)
                    {
                        zombies++;
                    }
                    else if (entity is EntityAnimal)
                    {
                        animals++;
                    }
                }
            }

            int onlinePlayerCount = world.Players.Count;
            int offlinePlayerCount = gameManager.GetPersistentPlayerList().Players.Count - onlinePlayerCount;
            return new Stats()
            {
                Uptime = Time.timeSinceLevelLoad,
                GameTime = new GameTime()
                {
                    Days = GameUtils.WorldTimeToDays(worldTime),
                    Hours = GameUtils.WorldTimeToHours(worldTime),
                    Minutes = GameUtils.WorldTimeToMinutes(worldTime),
                },
                Animals = animals,
                MaxAnimals = GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedAnimals),
                Zombies = zombies,
                MaxZombies = GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedZombies),
                Entities = world.Entities.Count,
                OnlinePlayers = onlinePlayerCount,
                MaxOnlinePlayers = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount),
                OfflinePlayers = offlinePlayerCount,
                IsBloodMoon = world.aiDirector.BloodMoonComponent.BloodMoonActive,
                FPS = gameManager.fps.Counter,
                Heap = (float)GC.GetTotalMemory(false) / 1048576F,
                MaxHeap = (float)GameManager.MaxMemoryConsumption / 1048576F,
                Chunks = Chunk.InstanceCount,
                CGO = world.m_ChunkManager.GetDisplayedChunkGameObjectsCount(),
                Items = EntityItem.ItemInstanceCount,
                ChunkObservedEntities = world.m_ChunkManager.m_ObservedEntities.Count,
                ResidentSetSize = (float)GetRSS.GetCurrentRSS() / 1048576F,
                ServerName = GamePrefs.GetString(EnumGamePrefs.ServerName),
                Region = GamePrefs.GetString(EnumGamePrefs.Region),
                Language = GamePrefs.GetString(EnumGamePrefs.Language),
                ServerVersion = global::Constants.cVersionInformation.LongString,
                ServerIp = GamePrefs.GetString(EnumGamePrefs.ServerIP),
                ServerPort = GamePrefs.GetInt(EnumGamePrefs.ServerPort),
                GameMode = GamePrefs.GetString(EnumGamePrefs.GameMode),
                GameWorld = GamePrefs.GetString(EnumGamePrefs.GameWorld),
                GameName = GamePrefs.GetString(EnumGamePrefs.GameName),
                GameDifficulty = GamePrefs.GetInt(EnumGamePrefs.GameDifficulty),
            };
        }

        /// <summary>
        ///系统信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(nameof(SystemInfo))]
        public Models.SystemInfo SystemInfo()
        {
            return new Models.SystemInfo()
            {
                DeviceModel = UnityEngine.Device.SystemInfo.deviceModel,
                DeviceName = UnityEngine.Device.SystemInfo.deviceName,
                DeviceType = UnityEngine.Device.SystemInfo.deviceType.ToString(),
                DeviceUniqueIdentifier = UnityEngine.Device.SystemInfo.deviceUniqueIdentifier,
                OperatingSystem = UnityEngine.Device.SystemInfo.operatingSystem,
                OperatingSystemFamily = UnityEngine.Device.SystemInfo.operatingSystemFamily.ToString(),
                ProcessorCount = UnityEngine.Device.SystemInfo.processorCount,
                ProcessorFrequency = UnityEngine.Device.SystemInfo.processorFrequency,
                ProcessorType = UnityEngine.Device.SystemInfo.processorType,
                SystemMemorySize = UnityEngine.Device.SystemInfo.systemMemorySize,
            };
        }

        /// <summary>
        /// Get game infomation.
        /// </summary>
        [HttpGet]
        [Route(nameof(GameInfo))]
        public JObject GameInfo()
        {
            var gsi = ConnectionManager.Instance.LocalServerInfo;
            var serverInfo = new JObject();

            foreach (string stringGamePref in Enum.GetNames(typeof(GameInfoString)))
            {
                string value = gsi.GetValue((GameInfoString)Enum.Parse(typeof(GameInfoString), stringGamePref));

                var singleStat = new JObject()
                {
                    { "type", "string" },
                    { "value", value }
                };

                serverInfo.Add(stringGamePref, singleStat);
            }

            foreach (string intGamePref in Enum.GetNames(typeof(GameInfoInt)))
            {
                int value = gsi.GetValue((GameInfoInt)Enum.Parse(typeof(GameInfoInt), intGamePref));

                var singleStat = new JObject()
                {
                    { "type", "int" },
                    { "value", value }
                };

                serverInfo.Add(intGamePref, singleStat);
            }

            foreach (string boolGamePref in Enum.GetNames(typeof(GameInfoBool)))
            {
                bool value = gsi.GetValue((GameInfoBool)Enum.Parse(typeof(GameInfoBool), boolGamePref));

                var singleStat = new JObject()
                {
                    { "type", "bool" },
                    { "value", value }
                };

                serverInfo.Add(boolGamePref, singleStat);
            }

            return serverInfo;
        }

        /// <summary>
        /// Get game infomation.
        /// </summary>
        [HttpPost]
        [Route(nameof(GiveItem))]
        public IEnumerable<string> GiveItem([FromBody]GiveItem giveItemEntry)
        {
            string cmd = string.Format("ty-gi {0} {1} {2} {3} {4}",
                Utilities.Utils.FormatCommandArgs(giveItemEntry.TargetPlayerIdOrName),
                giveItemEntry.ItemName,
                giveItemEntry.Count,
                giveItemEntry.Quality,
                giveItemEntry.Durability);

            return ExecuteConsoleCommand(cmd);
        }

        /// <summary>
        /// Send global message.
        /// </summary>
        [HttpPost]
        [Route(nameof(SendGlobalMessage))]
        public IEnumerable<string> SendGlobalMessage([FromBody] GlobalMessage globalMessage)
        {
            return Utilities.Utils.SendGlobalMessage(globalMessage);
        }

        /// <summary>
        /// Send private message.
        /// </summary>
        [HttpPost]
        [Route(nameof(SendPrivateMessage))]
        public IEnumerable<string> SendPrivateMessage([FromBody] PrivateMessage privateMessage)
        {
            return Utilities.Utils.SendPrivateMessage(privateMessage);
        }

        /// <summary>
        /// Restart server.
        /// </summary>
        [HttpPost]
        [Route(nameof(Restart))]
        public IEnumerable<string> Restart([FromUri]bool force = false)
        {
            string cmd = "ty-rs";
            if (force)
            {
                cmd += " -f";
            }

            return ExecuteConsoleCommand(cmd);
        }

        /// <summary>
        /// Shutdown server.
        /// </summary>
        [HttpDelete]
        [Route(nameof(Shutdown))]
        public IEnumerable<string> Shutdown()
        {
            return ExecuteConsoleCommand("shutdown");
        }

        /// <summary>
        /// Reset Player.
        /// </summary>
        [HttpDelete]
        [Route(nameof(ResetPlayer))]
        public IEnumerable<string> ResetPlayer([FromUri] string playerId)
        {
            return ExecuteConsoleCommand("ty-rpp " + playerId);
        }

        /// <summary>
        /// Get server settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(nameof(Settings))]
        public List<NameDescValue> Settings([FromUri]Language language)
        {
            string json = ModApi.GetDefaultConfigContent("serversettings.json", Locales.Get(language));
            var dict = JsonConvert.DeserializeObject<Dictionary<string, DescValue>>(json);

            if (dict == null)
            {
                throw new InvalidOperationException("Load settings faild, the json object is null.");
            }

            var result = new List<NameDescValue>();

            string path = Path.Combine(AppContext.BaseDirectory, ModApi.AppSettings.ServerSettingsFileName);
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            var xmlRoot = xmlDocument.DocumentElement;

            string name;
            string value;
            foreach (XmlNode node in xmlRoot.ChildNodes)
            {
                if (node.Attributes != null)
                {
                    name = node.Attributes["name"].Value;
                    value = node.Attributes["value"].Value;
                    result.Add(new NameDescValue()
                    {
                        Name = name,
                        Group = dict.ContainsKey(name) ? dict[name].Group : string.Empty,
                        Desc = dict.ContainsKey(name) ? dict[name].Desc : string.Empty,
                        Value = value
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Put server settings.
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route(nameof(Settings))]
        public IHttpActionResult Settings([FromBody] Dictionary<string, string> model)
        {
            string path = Path.Combine(AppContext.BaseDirectory, ModApi.AppSettings.ServerSettingsFileName);
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            var rootNode = xmlDocument.SelectSingleNode("ServerSettings");

            foreach (var item in model)
            {
                var node = rootNode.SelectSingleNode(string.Format("property[@name='{0}']", item.Key));
                if (node != null && node.Attributes != null)
                {
                    node.Attributes["value"].Value = item.Value;
                }
            }

            xmlDocument.Save(path);
            return Ok();
        }
    }
}
