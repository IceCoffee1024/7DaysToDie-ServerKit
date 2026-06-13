using SdtdServerKit.Managers;

namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Exports an Area to the directory {UserDataFolder}/LocalPrefabs.
    /// </summary>
    public class ExportPrefab : ConsoleCmdBase
    {
        /// <inheritdoc/>
        public override string getDescription()
        {
            return "Exports an Area to the directory {UserDataFolder}/LocalPrefabs.";
        }

        /// <inheritdoc/>
        public override string getHelp()
        {
            return "Usage:\n" +
                "  1. ty-ExportPrefab {x1} {y1} {z1} {x2} {y2} {z2} {prefabFileName} [overwrite]\n" +
                "  2. ty-ExportPrefab\n" +
                "  3. ty-ExportPrefab {prefabFileName} [overwrite]\n" +
                "1. Export the defined area to a prefabFile in folder {UserDataFolder}/LocalPrefabs\n" +
                "2. Store the player position to be used togheter on method 3.\n" +
                "3. Use stored position on method 2. with current position to export the area to prefab File in folder {UserDataFolder}/LocalPrefabs\n" +
                "NOTE: Sleepervolumes are lost during this process.";
        }

        /// <inheritdoc/>
        public override string[] getCommands()
        {
            return new string[]
            {
                "ty-ExportPrefab",
                "ty-ep"
            };
        }

        /// <inheritdoc/>
        public override void Execute(List<string> args, CommandSenderInfo senderInfo)
        {
            try
            {
                int x1, y1, z1, x2, y2, z2;
                string prefabFileName;
                if (args.Count >= 7)
                {
                    int.TryParse(args[0], out x1);
                    int.TryParse(args[1], out y1);
                    int.TryParse(args[2], out z1);
                    int.TryParse(args[3], out x2);
                    int.TryParse(args[4], out y2);
                    int.TryParse(args[5], out z2);
                    prefabFileName = args[6];
                }
                else if (args.Count == 0)
                {
                    int entityId = senderInfo.GetEntityId();
                    if (LivePlayerManager.TryGetByEntityId(entityId, out var managedPlayer) == false)
                    {
                        Log("ERR: Unable to get your position.");
                        return;
                    }

                    var playerPosition = managedPlayer!.EntityPlayer.GetBlockPosition();
                    _positionCache[entityId] = playerPosition;
                    Log("Stored position: " + playerPosition);
                    return;
                }
                else if(args.Count == 1 || args.Count == 2)
                {
                    prefabFileName = args[0];

                    int entityId = senderInfo.GetEntityId();
                    if (LivePlayerManager.TryGetByEntityId(entityId, out var managedPlayer) == false)
                    {
                        Log("ERR: Unable to get your position.");
                        return;
                    }

                    var playerPosition = managedPlayer!.EntityPlayer.GetBlockPosition();
                    (x1, y1, z1) = (playerPosition.x, playerPosition.y, playerPosition.z);

                    if (_positionCache.TryGetValue(entityId, out var storedPosition) == false)
                    {
                        Log("ERR: There isnt any stored location. Use method 2. to store a position.");
                        return;
                    }

                    (x2, y2, z2) = (storedPosition.x, storedPosition.y, storedPosition.z);
                }
                else
                {
                    Log("ERR: Wrong number of arguments.");
                    Log(this.GetHelp());
                    return;
                }

                if (ContainsCaseInsensitive(args, "overwrite") == false && Prefab.PrefabExists(prefabFileName))
                {
                    Log($"A prefab with the name: {prefabFileName} already exists.");
                    return;
                }

                var dir = Path.Combine(LaunchPrefs.UserDataFolder.Value, "LocalPrefabs");
                if (Directory.Exists(dir) == false)
                {
                    Directory.CreateDirectory(dir);
                }

                if (x2 < x1)
                {
                    (x1, x2) = (x2, x1);
                }
                if (y2 < y1)
                {
                    (y1, y2) = (y2, y1);
                }
                if (z2 < z1)
                {
                    (z1, z2) = (z2, z1);
                }

                var position1 = new Vector3i(x1, y1, z1);
                var position2 = new Vector3i(x2, y2, z2);

                var prefab = new Prefab()
                {
                    bCopyAirBlocks = true,
                };
                string fileNameWithExtension = prefabFileName + ".tts";
                prefab.location = new PathAbstractions.AbstractedLocation(PathAbstractions.EAbstractedLocationType.UserDataPath, fileNameWithExtension, Path.Combine(dir, fileNameWithExtension), null, true, null);
                prefab.copyFromWorld(GameManager.Instance.World, position1, position2);
                if (prefab.Save(prefab.location, true) == false)
                {
                    Log("Prefab could not be saved");
                    return;
                }

                Log($"Prefab {prefabFileName} exported. Area mapped from {position1} to {position2}");
            }
            catch (Exception ex)
            {
                Log("Error in ExportPrefab.Execute" + Environment.NewLine + ex.ToString());
            }
        }

        private static readonly Dictionary<int, Vector3i> _positionCache = new Dictionary<int, Vector3i>();
    }
}
