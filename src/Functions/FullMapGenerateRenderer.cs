using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using SdtdServerKit.Utilities;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// 渲染完整地图
    /// </summary>
    internal static class FullMapGenerateRenderer
    {
        private const int TileSize = MapTileRenderer.TileSize;        // 128
        private const int MaxZoom = MapTileRenderer.MaxZoom;          // 4
        private const int ChunkPixels = MapTileRenderer.ChunkPixels;  // 16
        private const int ChunksPerTile = MapTileRenderer.ChunksPerTile; // 8


        private const int DecorationOverlapChunks = 1;


        private static readonly object _terrainGenLock = new object();


        private static readonly object _decorateLock = new object();


        internal static volatile bool SuppressPoiResetLog;

        private static readonly object _lock = new object();
        private static Thread? _worker;
        private static volatile bool _stopRequested;

        private static string _status = "idle"; 
        private static int _chunksDone;
        private static int _chunksTotal;
        private static float _elapsedSeconds;
        private static string? _error;


        private const int ThrottleEveryNChunksBusy = 64;

        private const int ThrottleSleepMsBusy = 10;

        /// <summary>
        /// 在线人数的缓存值（由后台线程定期刷新），避免每个 chunk 都去触碰游戏的玩家集合。
        /// </summary>
        private static volatile int _cachedOnlinePlayers;

        /// <summary>上次刷新在线人数的时间戳</summary>
        private static long _lastPlayerCheckTicks;

        /// <summary>在线人数缓存刷新间隔（毫秒）。</summary>
        private const int PlayerCheckIntervalMs = 2000;

        /// <summary>关服钩子是否已注册（仅注册一次）。</summary>
        private static bool _shutdownHookRegistered;

        /// <summary>当前是否正在渲染。</summary>
        public static bool IsRunning
        {
            get
            {
                lock (_lock)
                {
                    return _worker != null && _worker.IsAlive;
                }
            }
        }

        /// <summary>
        /// 启动后台完整地图渲染
        /// </summary>
        public static bool Start()
        {
            lock (_lock)
            {
                if (_worker != null && _worker.IsAlive)
                {
                    return false;
                }

                World? world = null;
                ChunkProviderGenerateWorld? chunkProvider = null;
                ITerrainGenerator? terrainGenerator = null;
                int seed = 0;
                int minChunkX = 0, minChunkZ = 0, maxChunkX = 0, maxChunkZ = 0;
                string? saveGameDir = null;
                string? prepareError = null;

                void Prepare()
                {
                    try
                    {
                        world = GameManager.Instance?.World;
                        if (world == null)
                        {
                            prepareError = "World 尚未初始化";
                            return;
                        }

                        var chunkCache = world.ChunkCache;
                        if (chunkCache == null)
                        {
                            prepareError = "ChunkCache 尚未初始化";
                            return;
                        }

                        chunkProvider = chunkCache.ChunkProvider as ChunkProviderGenerateWorld;
                        if (chunkProvider == null)
                        {
                            prepareError = "当前世界的 ChunkProvider 不是可生成地形的类型（无法快速渲染完整地图）";
                            return;
                        }

                        terrainGenerator = chunkProvider.GetTerrainGenerator();
                        if (terrainGenerator == null)
                        {
                            prepareError = "地形生成器（ITerrainGenerator）为空";
                            return;
                        }

                        seed = world.Seed;

                        if (!world.GetWorldExtent(out Vector3i minPos, out Vector3i maxPos))
                        {
                            prepareError = "无法获取世界范围";
                            return;
                        }

                        minChunkX = World.toChunkXZ(Math.Min(minPos.x, maxPos.x));
                        maxChunkX = World.toChunkXZ(Math.Max(minPos.x, maxPos.x));
                        minChunkZ = World.toChunkXZ(Math.Min(minPos.z, maxPos.z));
                        maxChunkZ = World.toChunkXZ(Math.Max(minPos.z, maxPos.z));

                        saveGameDir = GameIO.GetSaveGameDir();
                    }
                    catch (Exception ex)
                    {
                        prepareError = ex.Message;
                        CustomLogger.Error(ex, "完整地图渲染：主线程预备阶段异常");
                    }
                }


                if (ThreadManager.IsMainThread())
                {
                    Prepare();
                }
                else
                {
                    var prepared = new ManualResetEventSlim(false);
                    ModApi.MainThreadSyncContext.Post(_ =>
                    {
                        try { Prepare(); }
                        finally { try { prepared.Set(); } catch { } }
                    }, null);

                    if (!prepared.Wait(15000))
                    {
                        _status = "error";
                        _error = "主线程预备超时（15s）";
                        CustomLogger.Error("完整地图渲染：主线程预备超时");
                        return false;
                    }
                }

                if (prepareError != null || world == null || chunkProvider == null || terrainGenerator == null || saveGameDir == null)
                {
                    _status = "error";
                    _error = prepareError ?? "主线程预备失败";
                    CustomLogger.Error($"完整地图渲染启动失败：{_error}");
                    return false;
                }

                _stopRequested = false;
                _status = "running";
                _chunksDone = 0;
                _chunksTotal = 0;
                _elapsedSeconds = 0f;
                _error = null;

                if (!_shutdownHookRegistered)
                {
                    ModEventHub.GameShutdown += OnGameShutdown;
                    _shutdownHookRegistered = true;
                }

                var ctx = new RenderContext(world, chunkProvider, terrainGenerator, seed,
                    minChunkX, minChunkZ, maxChunkX, maxChunkZ, saveGameDir);

                _worker = new Thread(() => RenderLoop(ctx))
                {
                    Name = "TianYiServerKit_FullMapGen",
                    IsBackground = true,
                };
                _worker.Start();
                return true;
            }
        }

        /// <summary>请求停止当前渲染</summary>
        public static void Stop()
        {
            _stopRequested = true;
            CustomLogger.Info("完整地图渲染：已发送停止指令");
        }

        /// <summary>获取当前渲染进度快照。</summary>
        public static (string status, int chunksDone, int chunksTotal, float elapsedSeconds, string? error) GetProgress()
        {
            lock (_lock)
            {
                return (_status, _chunksDone, _chunksTotal, _elapsedSeconds, _error);
            }
        }

        /// <summary>关服时停止渲染线程。</summary>
        private static void OnGameShutdown()
        {
            try
            {
                _stopRequested = true;
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "完整地图渲染：关服停止失败");
            }
        }

        private sealed class RenderContext
        {
            public readonly World World;
            public readonly ChunkProviderGenerateWorld ChunkProvider;
            public readonly ITerrainGenerator TerrainGenerator;
            public readonly int Seed;
            public readonly int MinChunkX;
            public readonly int MinChunkZ;
            public readonly int MaxChunkX;
            public readonly int MaxChunkZ;
            public readonly string SaveGameDir;

            public RenderContext(World world, ChunkProviderGenerateWorld chunkProvider, ITerrainGenerator terrainGenerator, int seed,
                int minChunkX, int minChunkZ, int maxChunkX, int maxChunkZ, string saveGameDir)
            {
                World = world;
                ChunkProvider = chunkProvider;
                TerrainGenerator = terrainGenerator;
                Seed = seed;
                MinChunkX = minChunkX;
                MinChunkZ = minChunkZ;
                MaxChunkX = maxChunkX;
                MaxChunkZ = maxChunkZ;
                SaveGameDir = saveGameDir;
            }
        }

        private static void RenderLoop(RenderContext ctx)
        {
            var sw = Stopwatch.StartNew();

            // 重置在线人数缓存
            _lastPlayerCheckTicks = 0;


            MapTileRenderer.SuppressIncremental = true;
            SuppressPoiResetLog = true;
            try
            {
                string mapDir = ctx.SaveGameDir + "/map";

                int minTileX = MapTileRenderer.FloorDiv(ctx.MinChunkX, ChunksPerTile);
                int maxTileX = MapTileRenderer.FloorDiv(ctx.MaxChunkX, ChunksPerTile);
                int minTileZ = MapTileRenderer.FloorDiv(ctx.MinChunkZ, ChunksPerTile);
                int maxTileZ = MapTileRenderer.FloorDiv(ctx.MaxChunkZ, ChunksPerTile);

                int widthChunks = ctx.MaxChunkX - ctx.MinChunkX + 1;
                int heightChunks = ctx.MaxChunkZ - ctx.MinChunkZ + 1;
                long total = (long)widthChunks * heightChunks;
                lock (_lock)
                {
                    _chunksTotal = total > int.MaxValue ? int.MaxValue : (int)total;
                }

                CustomLogger.Info($"完整地图渲染已启动：区块范围 X[{ctx.MinChunkX},{ctx.MaxChunkX}] " +
                    $"Z[{ctx.MinChunkZ},{ctx.MaxChunkZ}]，共 {total} 个区块，输出目录 {mapDir}");

                try
                {
                    if (Directory.Exists(mapDir))
                    {
                        Directory.Delete(mapDir, true);
                    }
                }
                catch (Exception ex)
                {
                    CustomLogger.Debug(ex, "完整地图渲染：清理旧地图目录失败（继续渲染）");
                }
                Directory.CreateDirectory(mapDir);
                MapTileRenderer.WriteMapInfoFile(mapDir);

                int leftPad = DecorationOverlapChunks;          
                const int rightPad = 1;                          
                int windowSize = ChunksPerTile + leftPad + rightPad;
                int decorateSize = ChunksPerTile + leftPad;     

                var rgba = new byte[TileSize * TileSize * 4];
                int sinceThrottle = 0;

                for (int tileX = minTileX; tileX <= maxTileX; tileX++)
                {
                    for (int tileZ = minTileZ; tileZ <= maxTileZ; tileZ++)
                    {
                        if (_stopRequested)
                        {
                            Finish("idle", sw, manuallyStopped: true);
                            return;
                        }

                        int baseChunkX = tileX * ChunksPerTile;
                        int baseChunkZ = tileZ * ChunksPerTile;
                        int originChunkX = baseChunkX - leftPad; 
                        int originChunkZ = baseChunkZ - leftPad;

                        var window = new Chunk?[windowSize, windowSize];
                        try
                        {
                            for (int ix = 0; ix < windowSize; ix++)
                            {
                                for (int iz = 0; iz < windowSize; iz++)
                                {
                                    if (_stopRequested)
                                    {
                                        Finish("idle", sw, manuallyStopped: true);
                                        return;
                                    }
                                    window[ix, iz] = GenerateBareChunk(ctx, originChunkX + ix, originChunkZ + iz);

                                    AdaptiveThrottle(ref sinceThrottle);
                                }
                            }

                            DecorateWindow(ctx, window, windowSize, decorateSize);

                            if (_stopRequested)
                            {
                                Finish("idle", sw, manuallyStopped: true);
                                return;
                            }

                            Array.Clear(rgba, 0, rgba.Length); 
                            bool tileHasData = false;

                            for (int dx = 0; dx < ChunksPerTile; dx++)
                            {
                                for (int dz = 0; dz < ChunksPerTile; dz++)
                                {
                                    int cx = baseChunkX + dx;
                                    int cz = baseChunkZ + dz;

                                    if (cx < ctx.MinChunkX || cx > ctx.MaxChunkX ||
                                        cz < ctx.MinChunkZ || cz > ctx.MaxChunkZ)
                                    {
                                        continue;
                                    }

                                    Chunk? center = window[leftPad + dx, leftPad + dz];

                                    lock (_lock) { _chunksDone++; }

                                    if (center == null) continue;

                                    ushort[]? colors = GetColorsSnapshot(center);
                                    if (colors != null)
                                    {
                                        MapTileRenderer.BlitChunkToTile(colors, rgba, dx, dz);
                                        tileHasData = true;
                                    }
                                }
                            }

                            if (tileHasData)
                            {
                                MapTileRenderer.SaveTile(mapDir, MaxZoom, tileX, tileZ, rgba);
                            }
                        }
                        finally
                        {
                            for (int ix = 0; ix < windowSize; ix++)
                            {
                                for (int iz = 0; iz < windowSize; iz++)
                                {
                                    var c = window[ix, iz];
                                    if (c != null)
                                    {
                                        try { MemoryPools.PoolChunks.FreeSync(c); }
                                        catch (Exception ex) { CustomLogger.Warn(ex, "完整地图渲染：归还临时 chunk 失败"); }
                                    }
                                }
                            }
                        }
                    }
                }

                if (_stopRequested)
                {
                    Finish("idle", sw, manuallyStopped: true);
                    return;
                }

                MapTileRenderer.BuildPyramidFromMaxZoom(mapDir, () => _stopRequested);

                if (_stopRequested)
                {
                    Finish("idle", sw, manuallyStopped: true);
                    return;
                }

                Finish("done", sw);
                CustomLogger.Info($"完整地图渲染完成，共 {_chunksDone} 个区块，耗时 {_elapsedSeconds:F1} 秒");
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    _status = "error";
                    _error = ex.Message;
                    _elapsedSeconds = (float)sw.Elapsed.TotalSeconds;
                }
                CustomLogger.Error(ex, "完整地图渲染异常");
            }
            finally
            {
                MapTileRenderer.SuppressIncremental = false;
                SuppressPoiResetLog = false;
            }
        }

        /// <summary>
        /// 根据当前在线玩家数决定是否让出 CPU
        /// </summary>
        private static void AdaptiveThrottle(ref int sinceThrottle)
        {
            if (GetOnlinePlayersCached() <= 0)
            {
                sinceThrottle = 0;
                return;
            }

            if (++sinceThrottle >= ThrottleEveryNChunksBusy)
            {
                sinceThrottle = 0;
                try { Thread.Sleep(ThrottleSleepMsBusy); } catch { }
            }
        }

        private static int GetOnlinePlayersCached()
        {
            long now = Stopwatch.GetTimestamp();
            long elapsedMs = (now - _lastPlayerCheckTicks) * 1000 / Stopwatch.Frequency;
            if (_lastPlayerCheckTicks != 0 && elapsedMs < PlayerCheckIntervalMs)
            {
                return _cachedOnlinePlayers;
            }
            _lastPlayerCheckTicks = now;

            try
            {
                var players = GameManager.Instance?.World?.Players;
                _cachedOnlinePlayers = players?.Count ?? 1;
            }
            catch
            {
                _cachedOnlinePlayers = 1; 
            }
            return _cachedOnlinePlayers;
        }


        private static Chunk? GenerateBareChunk(RenderContext ctx, int chunkX, int chunkZ)
        {
            Chunk? chunk = null;
            try
            {
                chunk = MemoryPools.PoolChunks.AllocSync(true);
                if (chunk == null)
                {
                    return null;
                }

                chunk.X = chunkX;
                chunk.Z = chunkZ;
                chunk.NeedsDecoration = true;

                GameRandom random = Utils.RandomFromSeedOnPos(chunkX, chunkZ, ctx.Seed);
                try
                {
                    lock (_terrainGenLock)
                    {
                        ctx.TerrainGenerator.GenerateTerrain(ctx.World, chunk, random);
                    }
                }
                finally
                {
                    GameRandomManager.Instance.FreeGameRandom(random);
                }

                return chunk;
            }
            catch (Exception ex)
            {
                CustomLogger.Debug(ex, $"完整地图渲染：生成裸地形 chunk ({chunkX},{chunkZ}) 失败，跳过");
                if (chunk != null)
                {
                    try { MemoryPools.PoolChunks.FreeSync(chunk); } catch { }
                }
                return null;
            }
        }


        private static void DecorateWindow(RenderContext ctx, Chunk?[,] window, int windowSize, int decorateSize)
        {
            try
            {
                lock (_decorateLock)
                {
                    var provider = ctx.ChunkProvider;
                    var prefabDecorator = provider.GetDynamicPrefabDecorator();
                    var decorators = provider.m_Decorators;

                    if (prefabDecorator != null)
                    {
                        for (int ix = 0; ix < decorateSize; ix++)
                        {
                            for (int iz = 0; iz < decorateSize; iz++)
                            {
                                Chunk? c = window[ix, iz];
                                if (c == null) continue;
                                try { prefabDecorator.DecorateChunk(ctx.World, c); }
                                catch (Exception ex) { CustomLogger.Warn(ex, $"完整地图渲染：POI 装饰 chunk ({c.X},{c.Z}) 失败"); }
                            }
                        }
                    }

                    for (int ix = 0; ix < decorateSize; ix++)
                    {
                        for (int iz = 0; iz < decorateSize; iz++)
                        {
                            Chunk? center = window[ix, iz];
                            Chunk? right = window[ix + 1, iz];
                            Chunk? up = window[ix, iz + 1];
                            if (center == null || right == null || up == null) continue;
                            try { provider.updateDecosAllowedForChunk(center, right, up); }
                            catch (Exception ex) { CustomLogger.Warn(ex, $"完整地图渲染：计算可装饰性 chunk ({center.X},{center.Z}) 失败"); }
                        }
                    }

                    if (decorators != null)
                    {
                        for (int ix = 0; ix < decorateSize; ix++)
                        {
                            for (int iz = 0; iz < decorateSize; iz++)
                            {
                                Chunk? center = window[ix, iz];
                                Chunk? right = window[ix + 1, iz];
                                Chunk? up = window[ix, iz + 1];
                                Chunk? upRight = window[ix + 1, iz + 1];
                                if (center == null || right == null || up == null || upRight == null) continue;

                                try
                                {
                                    for (int i = 0; i < decorators.Count; i++)
                                    {
                                        decorators[i].DecorateChunkOverlapping(ctx.World, center, right, up, upRight, ctx.Seed);
                                    }
                                    center.OnDecorated();
                                    center.NeedsDecoration = false;
                                }
                                catch (Exception ex)
                                {
                                    CustomLogger.Warn(ex, $"完整地图渲染：地表装饰 chunk ({center.X},{center.Z}) 失败");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "完整地图渲染：窗口装饰整体失败，将回退取裸地形色");
            }
        }

        private static ushort[]? GetColorsSnapshot(Chunk chunk)
        {
            try
            {
                ushort[]? colors = chunk.GetMapColors();
                if (colors == null || colors.Length != ChunkPixels * ChunkPixels)
                {
                    return null;
                }
                var snapshot = new ushort[colors.Length];
                Array.Copy(colors, snapshot, colors.Length);
                return snapshot;
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, $"完整地图渲染：取 chunk ({chunk.X},{chunk.Z}) 地图色失败");
                return null;
            }
        }

        private static void Finish(string status, Stopwatch sw, bool manuallyStopped = false)
        {
            lock (_lock)
            {
                _status = status;
                _elapsedSeconds = (float)sw.Elapsed.TotalSeconds;
            }
            if (manuallyStopped)
            {
                CustomLogger.Info("完整地图渲染已被手动停止");
            }
        }
    }
}
