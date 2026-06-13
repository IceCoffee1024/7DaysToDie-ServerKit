using SdtdServerKit.Managers;

namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Fill a defined area with a specific block.
    /// </summary>
    public class FillBlock : ConsoleCmdBase
    {
        /// <inheritdoc/>
        public override string getDescription()
        {
            return "Fill a defined area with a specific block.";
        }

        /// <inheritdoc/>
        public override string getHelp()
        {
            return "Usage:\n" +
                "  1. ty-FillBlock {blockIdOrName} {x1} {y1} {z1} {x2} {y2} {z2}\n" +
                "  2. ty-FillBlock\n" +
                "  3. ty-FillBlock {blockIdOrName}\n" +
                "1. Fill blocks with block name from x1,y1,z1 to x2,y2,z2\n" +
                "2. Store your position to be used on method 3. p1 store your position\n" +
                "3. Place blocks with block name from position stored on method 2 until your current location.";
        }

        /// <inheritdoc/>
        public override string[] getCommands()
        {
            return new string[]
            {
                "ty-FillBlock",
                "ty-fb"
            };
        }

        /// <inheritdoc/>
        public override void Execute(List<string> args, CommandSenderInfo senderInfo)
        {
            try
            {
                string blockIdOrName;
                int x1, y1, z1, x2, y2, z2;
                if (args.Count == 7)
                {
                    blockIdOrName = args[0];
                    int.TryParse(args[1], out x1);
                    int.TryParse(args[2], out y1);
                    int.TryParse(args[3], out z1);
                    int.TryParse(args[4], out x2);
                    int.TryParse(args[5], out y2);
                    int.TryParse(args[6], out z2);
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
                else if (args.Count == 1)
                {
                    blockIdOrName = args[0];

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

                if(Utilities.Utils.TryGetBlockValue(blockIdOrName, out var blockValue) == false)
                {
                    Log("ERR: Invalid block name or ID: " + blockIdOrName);
                    return;
                }

                var prefab = new Prefab(new Vector3i(x2 - x1 + 1, y2 - y1 + 1, z2 - z1 + 1))
                {
                    bCopyAirBlocks = true
                };
                var chunks = new HashSet<Chunk>();
                for (int x = x1; x <= x2; x++)
                {
                    for (int y = y1; y <= y2; y++)
                    {
                        for (int z = z1; z <= z2; z++)
                        {
                            if (GameManager.Instance.World.IsChunkAreaLoaded(x, y, z) == false)
                            {
                                Log("The prefab is too far away or target area is not loaded. Chunk not loaded on that area.");
                                return;
                            }
                            prefab.SetBlock(x - x1, y - y1, z - z1, blockValue);

                            var chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(x, y, z);
                            chunks.Add(chunk);
                        }
                    }
                }

                var prefabSize = prefab.size;
                var offsetPosition = new Vector3i(x1, y1, z1);

                var oldPrefab = new Prefab(prefab.size)
                {
                    bCopyAirBlocks = true
                };
                oldPrefab.copyFromWorld(GameManager.Instance.World, offsetPosition, offsetPosition + prefabSize);

                prefab.SleeperVolumes.Clear();
                prefab.CopyIntoLocal(GameManager.Instance.World.ChunkCache, offsetPosition, true, true, FastTags<TagGroup.Global>.none);

                ChunkHelper.ForceReload(chunks);

                ChunkHelper.CalculateStability(offsetPosition, prefabSize);

                UndoPrefab.SetUndo(senderInfo.GetEntityId(), oldPrefab, "Fill Block Area", offsetPosition);

                Log($"Fill block to {offsetPosition} - {offsetPosition + prefabSize}");
            }
            catch (Exception ex)
            {
                Log("Error in FillBlock.Execute" + Environment.NewLine + ex.ToString());
            }
        }        

        private static readonly Dictionary<int, Vector3i> _positionCache = new Dictionary<int, Vector3i>();
    }
}
