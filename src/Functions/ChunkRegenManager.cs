using Newtonsoft.Json;
using SdtdServerKit.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// 区块按需重生引擎（
    /// </summary>
    internal static class ChunkRegenManager
    {
        private static readonly object _lock = new object();

        /// <summary>
        /// 每个 chunk 的上次重置时间。Key = chunk key。
        /// </summary>
        private static Dictionary<long, DateTime> _chunkResetTimes = new Dictionary<long, DateTime>();

        /// <summary>
        /// 全局重置基准时间。chunk 时间戳早于此值即需要重置。
        /// </summary>
        private static DateTime _baselineResetTime = DateTime.Now;

        /// <summary>
        /// 是否已接管 GenerateChunks 线程
        /// </summary>
        private static volatile bool _threadTakenOver;

        /// <summary>
        /// 区域重置逻辑是否激活
        /// 关闭时接管线程仍在，但只走原版区块生成，行为与未安装本功能一致。
        /// </summary>
        private static volatile bool _resetEnabled;

        /// <summary>
        /// 自上次保存以来是否有变更（用于节流落盘）。
        /// </summary>
        private static volatile bool _dirty;

        // 接管线程时缓存的游戏对象引用
        private static ChunkProviderGenerateWorld? _cpgw;
        private static RegionFileManager? _regionFileManager;
        private static ITerrainGenerator? _terrainGenerator;
        private static World? _world;

        /// <summary>
        /// 区域重置逻辑是否激活。
        /// </summary>
        public static bool Enabled => _resetEnabled;

        /// <summary>
        /// 是否已接管区块生成线程。
        /// </summary>
        public static bool ThreadTakenOver => _threadTakenOver;

        /// <summary>
        /// 当前已纳管的 chunk 数量。
        /// </summary>
        public static int TrackedChunkCount
        {
            get
            {
                lock (_lock)
                {
                    return _chunkResetTimes.Count;
                }
            }
        }

        /// <summary>
        /// 启用引擎：接管线程、加载历史状态，并根据当前区域重建 chunk 集合。
        /// </summary>
        /// <param name="areas">当前所有重置区域</param>
        public static void EnableAreaReset(IEnumerable<T_ChunkResetArea> areas)
        {
            EnsureThreadTakenOver();
            Load();
            RebuildAreaStates(areas);
            _resetEnabled = true;
            CustomLogger.Info($"区域重置：接管 {TrackedChunkCount} 个 区块，基准时间 {_baselineResetTime:yyyy-MM-dd HH:mm:ss}");
        }

        /// <summary>
        /// 停用引擎并落盘（接管的线程保持运行，但只走原版生成，避免反复接管/还原线程）。
        /// </summary>
        public static void DisableAreaReset()
        {
            _resetEnabled = false;
            lock (_lock)
            {
                _chunkResetTimes.Clear();
                _dirty = true;
            }
            Save();
            CustomLogger.Info("区域重置：已停用");
        }

        /// <summary>
        /// 接管游戏的 GenerateChunks 线程
        /// </summary>
        private static void EnsureThreadTakenOver()
        {
            if (_threadTakenOver)
            {
                return;
            }
            lock (_lock)
            {
                if (_threadTakenOver)
                {
                    return;
                }
                try
                {
                    var world = GameManager.Instance?.World;
                    var cpgw = world?.ChunkCache?.ChunkProvider as ChunkProviderGenerateWorld;
                    if (world == null || cpgw == null || cpgw.m_RegionFileManager == null)
                    {
                        CustomLogger.Debug("区域重置(按需重生)：游戏世界尚未就绪，暂未接管区块生成线程，将在下次启用时重试");
                        return;
                    }

                    // 查找游戏原本的 GenerateChunks 线程
                    ThreadManager.ThreadInfo? original = null;
                    lock (ThreadManager.ActiveThreads)
                    {
                        foreach (var kv in ThreadManager.ActiveThreads)
                        {
                            if (kv.Key.EqualsCaseInsensitive("GenerateChunks"))
                            {
                                original = kv.Value;
                                break;
                            }
                        }
                    }

                    if (original == null)
                    {
                        CustomLogger.Error("区域重置(按需重生)：未找到游戏的 GenerateChunks 线程，接管失败，区域重置将不会生效");
                        return;
                    }

                    _cpgw = cpgw;
                    _world = world;
                    _regionFileManager = cpgw.m_RegionFileManager;
                    _terrainGenerator = cpgw.GetTerrainGenerator();

                    // 终止原线程并等待其干净退出，再用我们的循环函数替换
                    original.RequestTermination();
                    original.WaitForEnd(30);

                    cpgw.threadInfo = ThreadManager.StartThread(
                        "LSTY_GenerateChunks",
                        null,
                        new ThreadManager.ThreadFunctionLoopDelegate(GenerateChunksThreadReplacement),
                        null,
                        null,
                        null,
                        true,
                        false);

                    _threadTakenOver = true;
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// 替换后的区块生成线程主循环
        /// </summary>
        private static int GenerateChunksThreadReplacement(ThreadManager.ThreadInfo _threadInfo)
        {
            if (_threadInfo.TerminationRequested())
            {
                return -1;
            }

            var cpgw = _cpgw;
            var world = _world;
            if (cpgw == null || world == null || cpgw.m_RegionFileManager == null)
            {
                return 15;
            }

            long key;
            try
            {
                key = world.GetNextChunkToProvide();
                if (key == long.MaxValue)
                {
                    key = DynamicMeshThread.GetNextChunkToLoad();
                    if (key == long.MaxValue)
                    {
                        return 15;
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "区域重置(按需重生)：GetNextChunkToProvide 失败");
                return 100;
            }

            ChunkCluster cc = world.ChunkCache;
            try
            {
                if (_resetEnabled && ShouldReset(key))
                {
                    // 需要重置：自定义重生（删旧数据 + 重新生成全新地形）
                    if (RegenerateChunk(cc, key))
                    {
                        MarkReset(key);
                    }
                }
                else
                {
                    // 无需重置：走原版生成逻辑，与原版时序完全一致
                    cpgw.GenerateSingleChunk(cc, key, false);
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, $"区域重置(按需重生)：生成 chunk {key} 失败");
                return 15;
            }

            return 0;
        }

        /// <summary>
        /// 自定义重生单个 chunk：丢弃旧存档数据，重新生成全新地形并装饰。
        /// </summary>
        private static bool RegenerateChunk(ChunkCluster cc, long key)
        {
            // chunk 已加载（玩家正站在上面/正在使用）时绝不重生，直接跳过
            if (cc.ContainsChunkSync(key))
            {
                return false;
            }

            var cpgw = _cpgw!;
            var world = _world!;
            var rfm = _regionFileManager!;
            var terrainGenerator = _terrainGenerator;
            if (terrainGenerator == null)
            {
                return false;
            }

            if (rfm.ContainsChunkSync(key))
            {
                rfm.RemoveChunkSync(key);
            }

            int x = WorldChunkCache.extractX(key);
            int z = WorldChunkCache.extractZ(key);

            Chunk chunk = MemoryPools.PoolChunks.AllocSync(true);
            if (chunk == null)
            {
                return false;
            }

            chunk.X = x;
            chunk.Z = z;

            GameRandom gameRandom = Utils.RandomFromSeedOnPos(x, z, world.Seed);
            terrainGenerator.GenerateTerrain(world, chunk, gameRandom, Vector3i.zero, Vector3i.zero, false, false);
            GameRandomManager.Instance.FreeGameRandom(gameRandom);

            chunk.NeedsDecoration = true;
            chunk.NeedsLightCalculation = true;

            var decorator = cpgw.GetDynamicPrefabDecorator();
            if (decorator != null)
            {
                decorator.DecorateChunk(world, chunk);
            }

            bool added = cc.AddChunkSync(chunk, false);
            if (added)
            {
                cpgw.updateDecorationsWherePossible(chunk);
                chunk.isModified = true;
            }
            else
            {
                MemoryPools.PoolChunks.FreeSync(chunk);
            }

            return added;
        }

        /// <summary>
        /// 根据区域列表重建 chunk 状态字典
        /// </summary>
        public static void RebuildAreaStates(IEnumerable<T_ChunkResetArea> areas)
        {
            lock (_lock)
            {
                var newStates = new Dictionary<long, DateTime>();
                foreach (var area in areas)
                {
                    int chunkMinX = World.toChunkXZ(area.MinX);
                    int chunkMaxX = World.toChunkXZ(area.MaxX);
                    int chunkMinZ = World.toChunkXZ(area.MinZ);
                    int chunkMaxZ = World.toChunkXZ(area.MaxZ);

                    for (int cx = chunkMinX; cx <= chunkMaxX; cx++)
                    {
                        for (int cz = chunkMinZ; cz <= chunkMaxZ; cz++)
                        {
                            long key = WorldChunkCache.MakeChunkKey(cx, cz);
                            if (newStates.ContainsKey(key))
                            {
                                continue;
                            }
                            newStates[key] = _chunkResetTimes.TryGetValue(key, out var existing)
                                ? existing
                                : DateTime.Now;
                        }
                    }
                }
                _chunkResetTimes = newStates;
                _dirty = true;
            }
        }

        /// <summary>
        /// 把重置基准时间刷新为当前时间
        /// </summary>
        public static void BumpBaseline()
        {
            _baselineResetTime = DateTime.Now;
            _dirty = true;
        }

        /// <summary>
        /// 立即把所有 chunk 标记为需要重置
        /// </summary>
        public static void ResetAllAreaChunksImmediately()
        {
            lock (_lock)
            {
                var keys = new List<long>(_chunkResetTimes.Keys);
                foreach (var key in keys)
                {
                    _chunkResetTimes[key] = DateTime.MinValue;
                }
                _dirty = true;
            }
        }

        /// <summary>
        /// 判断指定 chunk 当前是否需要重置
        /// </summary>
        public static bool ShouldReset(long chunkKey)
        {
            lock (_lock)
            {
                return _chunkResetTimes.TryGetValue(chunkKey, out var lastReset)
                    && lastReset < _baselineResetTime;
            }
        }

        /// <summary>
        /// 标记指定 chunk 已完成重置
        /// </summary>
        public static void MarkReset(long chunkKey)
        {
            lock (_lock)
            {
                if (_chunkResetTimes.ContainsKey(chunkKey))
                {
                    _chunkResetTimes[chunkKey] = DateTime.Now;
                    _dirty = true;
                }
            }
        }

        #region 持久化

        private static string GetPersistFilePath()
        {
            string dir = Path.Combine(AppContext.BaseDirectory, "LSTY_Data");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return Path.Combine(dir, "chunkreset_state.json");
        }

        private static string? GetCurrentWorldName()
        {
            try
            {
                return GameManager.Instance?.World?.ChunkCache?.Name;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 如果有变更则落盘（由定时器节流调用）。
        /// </summary>
        public static void FlushIfDirty()
        {
            if (_dirty)
            {
                Save();
            }
        }

        /// <summary>
        /// 把当前状态保存到磁盘。
        /// </summary>
        public static void Save()
        {
            try
            {
                PersistModel model;
                lock (_lock)
                {
                    var chunks = new Dictionary<string, long>(_chunkResetTimes.Count);
                    foreach (var kv in _chunkResetTimes)
                    {
                        chunks[kv.Key.ToString()] = kv.Value.ToBinary();
                    }
                    model = new PersistModel
                    {
                        World = GetCurrentWorldName(),
                        Baseline = _baselineResetTime.ToBinary(),
                        Chunks = chunks
                    };
                    _dirty = false;
                }

                string json = JsonConvert.SerializeObject(model);
                File.WriteAllText(GetPersistFilePath(), json, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "区域重置(按需重生)：保存状态失败");
            }
        }

        /// <summary>
        /// 从磁盘加载状态。世界名不匹配时丢弃旧状态（避免跨存档误重置）。
        /// </summary>
        public static void Load()
        {
            try
            {
                string path = GetPersistFilePath();
                if (!File.Exists(path))
                {
                    lock (_lock)
                    {
                        _chunkResetTimes = new Dictionary<long, DateTime>();
                        _baselineResetTime = DateTime.Now;
                    }
                    return;
                }

                string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
                var model = JsonConvert.DeserializeObject<PersistModel>(json);
                string? currentWorld = GetCurrentWorldName();

                lock (_lock)
                {
                    if (model == null
                        || (currentWorld != null && model.World != null
                            && !string.Equals(model.World, currentWorld, StringComparison.OrdinalIgnoreCase)))
                    {
                        _chunkResetTimes = new Dictionary<long, DateTime>();
                        _baselineResetTime = DateTime.Now;
                        if (model != null)
                        {
                            CustomLogger.Debug($"区域重置(按需重生)：存档世界已变更（{model.World} -> {currentWorld}），重置状态已清空");
                        }
                        return;
                    }

                    _baselineResetTime = DateTime.FromBinary(model.Baseline);
                    var restored = new Dictionary<long, DateTime>(model.Chunks?.Count ?? 0);
                    if (model.Chunks != null)
                    {
                        foreach (var kv in model.Chunks)
                        {
                            if (long.TryParse(kv.Key, out long key))
                            {
                                try
                                {
                                    restored[key] = DateTime.FromBinary(kv.Value);
                                }
                                catch
                                {
                                    restored[key] = DateTime.MinValue;
                                }
                            }
                        }
                    }
                    _chunkResetTimes = restored;
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "区域重置(按需重生)：加载状态失败，使用空状态");
                lock (_lock)
                {
                    _chunkResetTimes = new Dictionary<long, DateTime>();
                    _baselineResetTime = DateTime.Now;
                }
            }
        }

        private class PersistModel
        {
            public string? World { get; set; }

            public long Baseline { get; set; }

            public Dictionary<string, long>? Chunks { get; set; }
        }

        #endregion
    }
}
