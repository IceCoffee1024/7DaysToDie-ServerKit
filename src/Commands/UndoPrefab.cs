namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Undo last prefab command.
    /// </summary>
    public class UndoPrefab : ConsoleCmdBase
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string getDescription()
        {
            return "Undo last prefab command.";
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string getHelp()
        {
            return "Usage:\n" +
                "  1. ty-UndoPrefab\n" +
                "  2. ty-UndoPrefab {id}\n" +
                "1. Undo prefabs command. Works with PlacePrefab, FillBlock, ReplaceBlock and DuplicateArea\n" +
                "2. Undo prefabs command by specified id.\n" +
                "NOTE: By default the size of undo history ise set to 10. You can change the undo history size using `ty-SetUndoSize`\n";
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string[] getCommands()
        {
            return new string[]
            {
                "ty-UndoPrefab",
                "ty-undo",
                "ty-up"
            };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Execute(List<string> args, CommandSenderInfo senderInfo)
        {
            try
            {
                if (args.Count == 0 || int.TryParse(args[0], out int id) == false)
                {
                    id = 0;
                }

                var undoPrefab = GetUndoPrefab(senderInfo.GetEntityId(), id);

                if (undoPrefab == null)
                {
                    Log("ERR: Unable to undo the last prefab command.");
                }
                else
                {
                    var prefab = undoPrefab.Prefab;
                    int prefabInstanceId = undoPrefab.PrefabInstanceId;
                    var prefabSize = prefab.size;
                    var offsetPosition = undoPrefab.OffsetPosition;

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

                    foreach (var chunk in chunks)
                    {
                        chunk.GetSleeperVolumes().Clear();
                    }

                    prefab.CopyIntoLocal(GameManager.Instance.World.ChunkCache, offsetPosition, true, true, FastTags<TagGroup.Global>.none);
                    
                    ChunkHelper.ForceReload(chunks);

                    ChunkHelper.CalculateStability(offsetPosition, prefabSize);

                    ChunkHelper.RemoveEntityInArea(offsetPosition.x, offsetPosition.z, offsetPosition.x + prefab.size.x, offsetPosition.z + prefab.size.z);

                    if (prefabInstanceId > 0)
                    {
                        if (ChunkHelper.RemovePrefabFromRWG(prefabInstanceId))
                        {
                            Log("Found undoBrender with `addToRWG`. Removed prefab with PrefabInstanceId={0} from Randomgen World.", prefabInstanceId);
                        }
                    }

                    Log($"Prefab Undone at {offsetPosition}");
                }
            }
            catch (Exception ex)
            {
                Log("Error in UndoPrefab.Execute" + Environment.NewLine + ex.ToString());
            }
        }        

        internal static UndoPrefabObj? GetUndoPrefab(int entityId, int id = 0)
        {
            if (_prefabCache.IsValueCreated == false
                || _prefabCache.Value.TryGetValue(entityId, out var list) == false 
                || list.Count == 0)
            {
                return null;
            }

            if(id < 0 || id >= list.Count)
            {
                return null;
            }

            var obj = list[id];
            list.RemoveAt(id);
            return obj;
        }

        internal static void SetUndo(int entityId, Prefab prefab, string prefabName, Vector3i offsetPosition, int prefabInstanceId = -1)
        {
            if (_prefabCache.Value.TryGetValue(entityId, out var list) == false)
            {
                list = new List<UndoPrefabObj>();
                _prefabCache.Value.Add(entityId, list);
            }

            if (list.Count >= _maxUndoHistorySize)
            {
                list.RemoveAt(list.Count - 1);
            }

            var item = new UndoPrefabObj(prefab, prefabName, offsetPosition, prefabInstanceId);
            list.Insert(0, item);
        }

        internal static List<UndoPrefabObj>? GetUndoHistoryList(int entityId = -1)
        {
            if (_prefabCache.IsValueCreated == false || _prefabCache.Value.TryGetValue(entityId, out var list) == false)
            {
                return null;
            }

            return list;
        }

        private static int _maxUndoHistorySize = 10;
        private static readonly Lazy<Dictionary<int, List<UndoPrefabObj>>> _prefabCache = new Lazy<Dictionary<int, List<UndoPrefabObj>>>();

        internal static void SetMaxUndoHistorySize(int maxUndoHistorySize, int entityId)
        {
            if(maxUndoHistorySize <= 0)
            {
                if (_prefabCache.IsValueCreated)
                {
                    if (entityId == -1)
                    {
                        _prefabCache.Value.Clear();
                    }
                    else
                    {
                        _prefabCache.Value.Remove(entityId);
                    }
                }
            }
            else
            {
                _maxUndoHistorySize = maxUndoHistorySize;
            }
        }

        internal static int GetMaxUndoHistorySize()
        {
            return _maxUndoHistorySize;
        }

        internal class UndoPrefabObj
        {
            public UndoPrefabObj(Prefab prefab, string prefabName, Vector3i offsetPosition, int prefabInstanceId)
            {
                this.Prefab = prefab;
                this.OffsetPosition = offsetPosition;
                this.PrefabInstanceId = prefabInstanceId;
                this.PrefabName = prefabName;
                this.CreatedAt = DateTime.Now;

                // Clear sleeper volumes
                prefab.SleeperVolumes.Clear();
            }

            public Prefab Prefab { get; set; }
            public string PrefabName { get; set; }
            public Vector3i OffsetPosition { get; set; }
            public DateTime CreatedAt { get; set; }
            public int PrefabInstanceId { get; set; }
        }
    }
}
