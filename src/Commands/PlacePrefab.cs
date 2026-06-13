using SdtdServerKit.Managers;

namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Places a Prefab on given location.
    /// </summary>
    public class PlacePrefab : ConsoleCmdBase
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string getDescription()
        {
            return "Places a Prefab on given location.";
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string getHelp()
        {
            return "Usage:\n" +
                "  1. ty-PlacePrefab {prefabFileName} {x} {y} {z} [noSleepers] [addToRWG]\n" +
                "  2. ty-PlacePrefab {prefabFileName} {x} {y} {z} {rot} [noSleepers] [addToRWG]\n" +
                "  3. ty-PlacePrefab {prefabFileName} [noSleepers] [addToRWG]\n" +
                "  4. ty-PlacePrefab {prefabFileName} {rot} [noSleepers] [addToRWG]\n" +
                "  5. ty-PlacePrefab {prefabFileName} {rot} {depth} [noSleepers] [addToRWG]\n" +
                "1. Places a prefab on {x} {y} {z} location\n" +
                "2. Places a prefab on {x} {y} {z} location with rot\n" +
                "3. Places a prefab on your position\n" +
                "4. Places a prefab on your position with rot\n" +
                "5. Places a prefab on your position with rot and y deslocated (depth blocks)\n" +
                "NOTE: {rot} means rotate the prefab to the left, must be equal to 0=0°, 1=90°, 2=180° or 3=270°\n" +
                "NOTE: Sleeper control is ONLY possible on prefabs that are present in prefabs.xml (world folder) that is used to create the map (RWG).\n" +
                "NOTE: Use parameter `addToRWG` to permanently add this prefab to the current RWG world. Can be reset like any other RWG prefab and will still be in world after a wipe. Will cause re-download of world for clients!\n" +
                "NOTE: Runtime search and load from {UserDataFolder}/LocalPrefabs\n" +
                "NOTE: If {prefabFileName} is a full file name that actually exists, it will be loaded exactly";
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string[] getCommands()
        {
            return new string[]
            {
                "ty-PlacePrefab",
                "ty-pp"
            };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Execute(List<string> args, CommandSenderInfo senderInfo)
        {
            try
            {
                //CustomLogger.Warn("Is Main: " + ThreadManager.IsMainThread());
                if (args.Count < 1 || args.Count > 7)
                {
                    Log($"ERR: Wrong number of arguments, expected 1 to 7, found {args.Count}.");
                    Log(this.GetHelp());
                    return;
                }

                bool addToRWG = ContainsCaseInsensitive(args, "addtorwg");
                bool noSleepers = ContainsCaseInsensitive(args, "nosleepers");
                if (addToRWG)
                {
                    args.RemoveAll(i => string.Equals(i, "addtorwg", StringComparison.OrdinalIgnoreCase));
                }
                if (noSleepers)
                {
                    args.RemoveAll(i => string.Equals(i, "nosleepers", StringComparison.OrdinalIgnoreCase));
                }

                string prefabName = args[0];
                string? runtimeSearchDir = null;
                bool isExactlyLoad = File.Exists(prefabName);
                if (isExactlyLoad)
                {
                    runtimeSearchDir = Path.GetDirectoryName(prefabName);
                    prefabName = Path.GetFileNameWithoutExtension(prefabName);
                }
                else
                {
                    var dir = new DirectoryInfo(Path.Combine(LaunchPrefs.UserDataFolder.Value, "LocalPrefabs"));
                    runtimeSearchDir = dir.FullName;
                }

                int x, y, z, rot = 0;
                if (args.Count == 4)
                {
                    x = int.Parse(args[1]);
                    y = int.Parse(args[2]);
                    z = int.Parse(args[3]);
                }
                else if(args.Count == 5)
                {
                    x = int.Parse(args[1]);
                    y = int.Parse(args[2]);
                    z = int.Parse(args[3]);
                    rot = int.Parse(args[4]);
                }
                else
                {
                    int entityId = senderInfo.GetEntityId();
                    if (entityId == -1)
                    {
                        Log("ERR: This command can be only sent by player in game.");
                        return;
                    }
                    if (LivePlayerManager.TryGetByEntityId(entityId, out var managedPlayer) == false)
                    {
                        Log("ERR: Unable to get your position.");
                        return;
                    }

                    var playerBlockPosition = managedPlayer!.EntityPlayer.GetBlockPosition();
                    x = playerBlockPosition.x;
                    y = playerBlockPosition.y;
                    z = playerBlockPosition.z;

                    if(args.Count >= 2)
                    {
                        rot = int.Parse(args[1]);
                        if (rot < 0 || rot > 3)
                        {
                            Log("ERR: Invalid rotation parameter. It need to be 0,1,2 or 3.");
                            return;
                        }

                        if (args.Count == 3)
                        {
                            int depth = int.Parse(args[2]);
                            y += depth;
                        }
                    }
                }

                var prefab = new Prefab()
                {
                    bCopyAirBlocks = true
                };
                if (isExactlyLoad || prefab.Load(prefabName, true, true, false, false) == false)
                {
                    Log("Try loading prefab from " + runtimeSearchDir);
                    var abstractedLocation = new PathAbstractions.AbstractedLocation(PathAbstractions.EAbstractedLocationType.UserDataPath, prefabName, runtimeSearchDir, null, prefabName, ".tts", true, null);
                    if (prefab.Load(abstractedLocation, true, true, false, false) == false)
                    {
                        Log("ERR: Unable to load prefab " + prefabName);
                        return;
                    }
                }

                Log("Rendering..., please wait.");

                y += prefab.yOffset;
                prefab.RotateY(true, rot);

                var prefabSize = prefab.size;
                var offsetPosition = new Vector3i(x, y, z);

                IEnumerable<Chunk>? chunks;
                try
                {
                    chunks = ChunkHelper.GetChunksInArea(offsetPosition, prefabSize);
                }
                catch (Exception ex)
                {
                    Log("ERR: " + ex.Message);
                    return;
                }

                var oldPrefab = new Prefab(prefabSize)
                {
                    bCopyAirBlocks = true
                };
                oldPrefab.copyFromWorld(GameManager.Instance.World, offsetPosition, offsetPosition + prefabSize);
                
                if (noSleepers)
                {
                    prefab.SleeperVolumes.Clear();
                }
                prefab.CopyIntoLocal(GameManager.Instance.World.ChunkCache, offsetPosition, true, true, FastTags<TagGroup.Global>.none);

                ChunkHelper.ForceReload(chunks);

                ChunkHelper.CalculateStability(offsetPosition, prefabSize);

                int prefabInstanceId = -1;
                if (addToRWG)
                {
                    prefabInstanceId = ChunkHelper.AddPrefabToRWG(prefab, offsetPosition);
                }

                UndoPrefab.SetUndo(senderInfo.GetEntityId(), oldPrefab, prefabName, offsetPosition, prefabInstanceId);

                Log($"Prefab {prefabName} loaded at {offsetPosition}");
            }
            catch (Exception ex)
            {
                Log("Error in PlacePrefab.Execute" + Environment.NewLine + ex.ToString());
            }
        }
    }
}
