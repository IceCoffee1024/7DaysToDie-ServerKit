using SdtdServerKit.Managers;

namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Duplicates an Area to another location.
    /// </summary>
    public class DuplicateArea : ConsoleCmdBase
    {
        /// <inheritdoc/>
        public override string getDescription()
        {
            return "Duplicates an Area to another location.";
        }

        /// <inheritdoc/>
        public override string getHelp()
        {
            return "Usage:\n" +
                "  1. ty-DuplicateArea {x1} {y1} {z1} {x2} {y2} {z2} {x} {y} {z}\n" +
                "  2. ty-DuplicateArea {x1} {y1} {z1} {x2} {y2} {z2} {x} {y} {z} {rot}\n" +
                "  3. ty-DuplicateArea p1\n" +
                "  4. ty-DuplicateArea p2\n" +
                "  5. ty-DuplicateArea {x} {y} {z}\n" +
                "  6. ty-DuplicateArea {x} {y} {z} {rot}\n" +
                "  7. ty-DuplicateArea\n" +
                "  8. ty-DuplicateArea {rot}\n" +
                "1. Duplicate the defined area on x,y,z\n" +
                "2. Duplicate the defined area on x,y,z with rot\n" +
                "3. Store on position 1 your current position\n" +
                "4. Store on position 2 your current position\n" +
                "5. Use stored position 1 and 2 to duplicate on x,y,z\n" +
                "6. Use stored position 1 and 2 to duplicate on x,y,z with rot\n" +
                "7. Use stored position 1 and 2 to duplicate on your current position\n" +
                "8. Use stored position 1 and 2 to duplicate on your current position with rot\n" +
                "NOTE: {rot} means rotate the prefab to the left, must be equal to 0=0°, 1=90°, 2=180° or 3=270°" +
                "NOTE: Sleepervolumes are lost during this process.";
        }

        /// <inheritdoc/>
        public override string[] getCommands()
        {
            return new string[]
            {
                "ty-DuplicateArea",
                "ty-da"
            };
        }

        /// <inheritdoc/>
        public override void Execute(List<string> args, CommandSenderInfo senderInfo)
        {
            try
            {
                int x1, y1, z1, x2, y2, z2, dest_x, dest_y, dest_z, rot = 0;
                int entityId = senderInfo.GetEntityId();
                if (args.Count >= 9)
                {
                    int.TryParse(args[0], out x1);
                    int.TryParse(args[1], out y1);
                    int.TryParse(args[2], out z1);
                    int.TryParse(args[3], out x2);
                    int.TryParse(args[4], out y2);
                    int.TryParse(args[5], out z2);
                    int.TryParse(args[6], out dest_x);
                    int.TryParse(args[7], out dest_y);
                    int.TryParse(args[8], out dest_z);

                    if (args.Count == 10)
                    {
                        int.TryParse(args[9], out rot);
                    }
                }                
                else if (args.Count <= 1)
                {
                    if (LivePlayerManager.TryGetByEntityId(entityId, out var managedPlayer) == false)
                    {
                        Log("ERR: Unable to get your position.");
                        return;
                    }

                    var playerPosition = managedPlayer!.EntityPlayer.GetBlockPosition();

                    if (args.Count == 1)
                    {
                        string text = args[0];
                        if (string.Equals(text, "p1", StringComparison.OrdinalIgnoreCase))
                        {
                            _position1Cache[entityId] = playerPosition;
                            Log($"Stored position 1: {playerPosition}");
                            return;
                        }
                        else if (string.Equals(text, "p2", StringComparison.OrdinalIgnoreCase))
                        {
                            _position2Cache[entityId] = playerPosition;
                            Log($"Stored position 2: {playerPosition}");
                            return;
                        }
                        else
                        {
                            int.TryParse(text, out rot);
                        }
                    }

                    if (TryGetPosition(entityId, out x1, out y1, out z1, out x2, out y2, out z2) == false)
                    {
                        return;
                    }

                    (dest_x, dest_y, dest_z) = (playerPosition.x, playerPosition.y, playerPosition.z);
                }
                else if (args.Count <= 4)
                {
                    if(TryGetPosition(entityId, out x1, out y1, out z1, out x2, out y2, out z2) == false)
                    {
                        return;
                    }

                    int.TryParse(args[0], out dest_x);
                    int.TryParse(args[1], out dest_y);
                    int.TryParse(args[2], out dest_z);

                    if(args.Count == 4)
                    {
                        int.TryParse(args[3], out rot);
                    }
                }
                else
                {
                    Log("ERR: Wrong number of arguments.");
                    Log(this.GetHelp());
                    return;
                }

                // Ensures x1 is the smaller value, x2 is the larger value, and so are y1 and y2, z1 and z2.
                if (x2 < x1)
                {
                    (x1, x2) = (x2, x1);
                }
                if(y2 < y1) 
                {
                    (y1, y2) = (y2, y1);
                }
                if (z2 < z1)
                {
                    (z1, z2) = (z2, z1);
                }

                var prefab = new Prefab()
                {
                    bCopyAirBlocks = true
                };
                prefab.copyFromWorld(GameManager.Instance.World, new Vector3i(x1, y1, z1), new Vector3i(x2, y2, z2));
                
                Log($"Area duplicated from {x1}, {y1}, {z1} to {x2}, {y2}, {z2}");
                prefab.RotateY(true, rot);

                var prefabSize = prefab.size;
                var offsetPosition = new Vector3i(dest_x, dest_y, dest_z);

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
                    bCopyAirBlocks = true,
                };
                oldPrefab.copyFromWorld(GameManager.Instance.World, offsetPosition, offsetPosition + prefabSize);
                
                prefab.SleeperVolumes.Clear();
                prefab.CopyIntoLocal(GameManager.Instance.World.ChunkCache, offsetPosition, true, true, FastTags<TagGroup.Global>.none);

                ChunkHelper.ForceReload(chunks);

                ChunkHelper.CalculateStability(offsetPosition, prefabSize);

                UndoPrefab.SetUndo(entityId, oldPrefab, "Duplicated Area", offsetPosition);

                Log($"Duplicated Area at {offsetPosition}");
            }
            catch (Exception ex)
            {
                Log("Error in DuplicateArea.Execute" + Environment.NewLine + ex.ToString());
            }
        }

        private static readonly Dictionary<int, Vector3i> _position1Cache = new Dictionary<int, Vector3i>();
        private static readonly Dictionary<int, Vector3i> _position2Cache = new Dictionary<int, Vector3i>();

        private bool TryGetPosition(int entityId, out int x1, out int y1, out int z1, out int x2, out int y2, out int z2)
        {
            x1 = y1 = z1 = x2 = y2 = z2 = 0;

            if (_position1Cache.TryGetValue(entityId, out var position1) == false)
            {
                Log("ERR: There isnt any stored position1, use method 3 to store a position.");
                Log(this.GetHelp());
                return false;
            }

            if (_position2Cache.TryGetValue(entityId, out var position2) == false)
            {
                Log("ERR: There isnt any stored position2, use method 4 to store a position.");
                Log(this.GetHelp());
                return false;
            }

            (x1, y1, z1) = (position1.x, position1.y, position1.z);
            (x2, y2, z2) = (position2.x, position2.y, position2.z);

            return true;
        }
    }
}
