using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using SdtdServerKit.Utilities;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// 后台线程全图地图渲染器
    /// </summary>
    internal static class MapTileRenderer
    {
        public const int TileSize = 128;

        public const int ZoomLevels = 5;

        internal const int MaxZoom = ZoomLevels - 1;

        internal const int ChunkPixels = 16;

        internal const int ChunksPerTile = TileSize / ChunkPixels;

        private static readonly object _lock = new object();
        private static Thread? _worker;
        private static volatile bool _stopRequested;

        private static string _status = "idle"; 
        private static int _chunksDone;
        private static int _chunksTotal;
        private static float _elapsedSeconds;
        private static string? _error;

        private const int ThrottleSleepMs = 15;

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
        /// 启动后台全图渲染。若已有任务在跑则返回 false。
        /// </summary>
        public static bool Start(string saveGameDir, string regionDir)
        {
            lock (_lock)
            {
                if (_worker != null && _worker.IsAlive)
                {
                    return false;
                }

                _stopRequested = false;
                _status = "running";
                _chunksDone = 0;
                _chunksTotal = 0;
                _elapsedSeconds = 0f;
                _error = null;

                _worker = new Thread(() => RenderLoop(saveGameDir, regionDir))
                {
                    Name = "TianYiServerKit_MapTileRenderer",
                    IsBackground = true,
                };
                _worker.Start();
                return true;
            }
        }

        public static void Stop()
        {
            _stopRequested = true;
        }

        public static (string status, int chunksDone, int chunksTotal, float elapsedSeconds, string? error) GetProgress()
        {
            lock (_lock)
            {
                return (_status, _chunksDone, _chunksTotal, _elapsedSeconds, _error);
            }
        }



        private static readonly object _incrementalLock = new object();
        private static Thread? _incrementalWorker;
        private static volatile bool _incrementalStop;
        private static string? _incrementalSaveDir;

        /// <summary>
        /// 抑制增量更新入队的开关
        /// </summary>
        internal static volatile bool SuppressIncremental;
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<long, ushort[]> _dirtyChunks
            = new System.Collections.Concurrent.ConcurrentDictionary<long, ushort[]>();

        private const int IncrementalFlushIntervalMs = 3000;

        /// <summary>
        /// 启用增量地图更新
        /// </summary>
        public static void EnableIncremental(string saveGameDir)
        {
            lock (_incrementalLock)
            {
                _incrementalSaveDir = saveGameDir;
                if (_incrementalWorker != null && _incrementalWorker.IsAlive)
                {
                    return;
                }
                _incrementalStop = false;
                _incrementalWorker = new Thread(IncrementalLoop)
                {
                    Name = "TianYiServerKit_MapIncremental",
                    IsBackground = true,
                };
                _incrementalWorker.Start();

                if (!_shutdownHookRegistered)
                {
                    ModEventHub.GameShutdown += OnGameShutdownFlush;
                    _shutdownHookRegistered = true;
                }

            }
        }

        private static bool _shutdownHookRegistered;

        private static void OnGameShutdownFlush()
        {
            try
            {
                _incrementalStop = true;
                if (!IsRunning && !_dirtyChunks.IsEmpty)
                {
                    FlushDirtyChunks();
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "地图增量更新：关服 flush 失败");
            }
        }


        public static void OnChunkColorsDone(Chunk chunk)
        {
            try
            {
                if (_incrementalSaveDir == null || chunk == null)
                {
                    return;
                }

                if (IsRunning || SuppressIncremental)
                {
                    return;
                }

                ushort[]? colors = chunk.GetMapColors();
                if (colors == null || colors.Length != ChunkPixels * ChunkPixels)
                {
                    return;
                }

                var snapshot = new ushort[colors.Length];
                Array.Copy(colors, snapshot, colors.Length);

                long key = WorldChunkCache.MakeChunkKey(chunk.X, chunk.Z);
                _dirtyChunks[key] = snapshot;
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "地图增量更新：入队 chunk 颜色失败");
            }
        }

        private static void IncrementalLoop()
        {
            while (!_incrementalStop)
            {
                try
                {
                    Thread.Sleep(IncrementalFlushIntervalMs);
                    if (_incrementalStop) break;
                    if (_dirtyChunks.IsEmpty) continue;
                    if (IsRunning) continue;

                    FlushDirtyChunks();
                }
                catch (Exception ex)
                {
                    CustomLogger.Error(ex, "地图增量更新后台线程异常");
                    try { Thread.Sleep(500); } catch { }
                }
            }
        }


        private static void FlushDirtyChunks()
        {
            string? saveDir = _incrementalSaveDir;
            if (saveDir == null) return;
            string mapDir = saveDir + "/map";

            var batch = new System.Collections.Generic.Dictionary<long, ushort[]>();
            foreach (var kv in _dirtyChunks)
            {
                if (_dirtyChunks.TryRemove(kv.Key, out var snapshot))
                {
                    batch[kv.Key] = snapshot;
                }
            }
            if (batch.Count == 0) return;

            var tileGroups = new System.Collections.Generic.Dictionary<long, System.Collections.Generic.List<long>>();
            foreach (long chunkKey in batch.Keys)
            {
                int cx = WorldChunkCache.extractX(chunkKey);
                int cz = WorldChunkCache.extractZ(chunkKey);
                int tileX = FloorDiv(cx, ChunksPerTile);
                int tileZ = FloorDiv(cz, ChunksPerTile);
                long tileKey = PackTileKey(tileX, tileZ);
                if (!tileGroups.TryGetValue(tileKey, out var list))
                {
                    list = new System.Collections.Generic.List<long>();
                    tileGroups[tileKey] = list;
                }
                list.Add(chunkKey);
            }

            var rgba = new byte[TileSize * TileSize * 4];
            int updatedTiles = 0;
            foreach (var group in tileGroups)
            {
                UnpackTileKey(group.Key, out int tileX, out int tileZ);
                string tileFile = $"{mapDir}/{MaxZoom}/{tileX}/{tileZ}.png";

                Array.Clear(rgba, 0, rgba.Length);
                if (File.Exists(tileFile))
                {
                    try
                    {
                        byte[] existing = File.ReadAllBytes(tileFile);
                        if (PurePng.TryDecode(existing, out int w, out int h, out byte[]? decoded)
                            && decoded != null && w == TileSize && h == TileSize)
                        {
                            Buffer.BlockCopy(decoded, 0, rgba, 0, rgba.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.Warn(ex, $"地图增量更新：读取瓦片 z{MaxZoom}/{tileX}/{tileZ} 失败，将以空白重建");
                    }
                }

                int baseChunkX = tileX * ChunksPerTile;
                int baseChunkZ = tileZ * ChunksPerTile;
                foreach (long chunkKey in group.Value)
                {
                    int cx = WorldChunkCache.extractX(chunkKey);
                    int cz = WorldChunkCache.extractZ(chunkKey);
                    int dx = cx - baseChunkX;
                    int dz = cz - baseChunkZ;
                    if (dx < 0 || dx >= ChunksPerTile || dz < 0 || dz >= ChunksPerTile) continue;
                    BlitChunkToTile(batch[chunkKey], rgba, dx, dz);
                }

                SaveTile(mapDir, MaxZoom, tileX, tileZ, rgba);
                PropagateUp(mapDir, tileX, tileZ);
                updatedTiles++;
            }

            CustomLogger.Debug($"地图增量更新：合并 {batch.Count} 个 chunk，更新 {updatedTiles} 个瓦片");
        }

        /// <summary>
        /// 最细层某瓦片更新后，沿金字塔向上逐级重建受影响的上层瓦片，直到 zoom 0。
        /// </summary>
        private static void PropagateUp(string mapDir, int maxZoomTileX, int maxZoomTileZ)
        {
            int tx = maxZoomTileX;
            int tz = maxZoomTileZ;
            for (int z = MaxZoom; z > 0; z--)
            {
                int dstZoom = z - 1;
                int dstTileX = FloorDiv(tx, 2);
                int dstTileZ = FloorDiv(tz, 2);
                RebuildOneUpperTile(mapDir, z, dstZoom, dstTileX, dstTileZ);
                tx = dstTileX;
                tz = dstTileZ;
            }
        }

        /// <summary>
        /// 由下层（srcZoom）的 2×2 子瓦片重建上层（dstZoom）的单个瓦片 (dstTileX,dstTileZ)。
        /// </summary>
        private static void RebuildOneUpperTile(string mapDir, int srcZoom, int dstZoom, int dstTileX, int dstTileZ)
        {
            var halfBuf = new byte[(TileSize / 2) * (TileSize / 2) * 4];
            var dstRgba = new byte[TileSize * TileSize * 4];
            Array.Clear(dstRgba, 0, dstRgba.Length);
            bool any = false;

            for (int sxOff = 0; sxOff <= 1; sxOff++)
            {
                for (int syOff = 0; syOff <= 1; syOff++)
                {
                    int sx = dstTileX * 2 + sxOff;
                    int sy = dstTileZ * 2 + syOff;
                    string srcFile = $"{mapDir}/{srcZoom}/{sx}/{sy}.png";
                    if (!File.Exists(srcFile)) continue;

                    byte[] srcBytes;
                    try { srcBytes = File.ReadAllBytes(srcFile); }
                    catch { continue; }

                    if (!PurePng.TryDecode(srcBytes, out int sw, out int sh, out byte[]? srcRgba)
                        || srcRgba == null || sw != TileSize || sh != TileSize)
                    {
                        continue;
                    }

                    HalfScale(srcRgba, halfBuf);
                    int dstPxX = sxOff * (TileSize / 2);
                    int dstPxRowTop = (1 - syOff) * (TileSize / 2);
                    CopyHalfIntoTile(halfBuf, dstRgba, dstPxX, dstPxRowTop);
                    any = true;
                }
            }

            if (any)
            {
                SaveTile(mapDir, dstZoom, dstTileX, dstTileZ, dstRgba);
            }
        }


        private static void RenderLoop(string saveGameDir, string regionDir)
        {
            var sw = Stopwatch.StartNew();
            RegionFileManager? rfm = null;
            try
            {
                string mapDir = saveGameDir + "/map";

                rfm = new RegionFileManager(regionDir, regionDir, 0, false);

                long[] allKeys = rfm.GetAllChunkKeys();
                if (allKeys.Length == 0)
                {
                    Finish("done", sw, "存档中没有任何 chunk，无需渲染");
                    return;
                }

                // 计算世界 chunk 范围
                int minChunkX = int.MaxValue, minChunkZ = int.MaxValue;
                int maxChunkX = int.MinValue, maxChunkZ = int.MinValue;
                foreach (long key in allKeys)
                {
                    int cx = WorldChunkCache.extractX(key);
                    int cz = WorldChunkCache.extractZ(key);
                    if (cx < minChunkX) minChunkX = cx;
                    if (cx > maxChunkX) maxChunkX = cx;
                    if (cz < minChunkZ) minChunkZ = cz;
                    if (cz > maxChunkZ) maxChunkZ = cz;
                }

                lock (_lock)
                {
                    _chunksTotal = allKeys.Length;
                }

                CustomLogger.Info($"全图渲染已启动：chunk 范围 X[{minChunkX},{maxChunkX}] Z[{minChunkZ},{maxChunkZ}]，" +
                    $"共 {allKeys.Length} 个 chunk，输出目录 {mapDir}");

                // 清空旧地图目录，重建
                try
                {
                    if (Directory.Exists(mapDir))
                    {
                        Directory.Delete(mapDir, true);
                    }
                }
                catch (Exception ex)
                {
                    CustomLogger.Warn(ex, "全图渲染：清理旧地图目录失败（继续渲染）");
                }
                Directory.CreateDirectory(mapDir);
                WriteMapInfo(mapDir);

                // 第一步：渲染最细层（zoom = MaxZoom）。逐瓦片流式处理，单瓦片仅占 128×128×4 = 64KB
                bool completed = RenderMaxZoomLevel(rfm, mapDir, minChunkX, minChunkZ, maxChunkX, maxChunkZ);
                if (!completed)
                {
                    Finish("idle", sw, null, manuallyStopped: true);
                    return;
                }

                // 第二步：自最细层向上逐级 1/2 降采样，生成 zoom = MaxZoom-1 ... 0
                for (int z = MaxZoom; z > 0; z--)
                {
                    if (_stopRequested)
                    {
                        Finish("idle", sw, null, manuallyStopped: true);
                        return;
                    }
                    BuildLowerZoomLevel(mapDir, z);
                }

                Finish("done", sw, null);
                CustomLogger.Info($"全图渲染完成，共 {_chunksDone} 个 chunk，耗时 {_elapsedSeconds:F1} 秒");
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    _status = "error";
                    _error = ex.Message;
                    _elapsedSeconds = (float)sw.Elapsed.TotalSeconds;
                }
                CustomLogger.Error(ex, "全图渲染异常");
            }
            finally
            {
                try { rfm?.Cleanup(); } catch { /* 忽略清理异常 */ }
            }
        }

        /// <summary>
        /// 渲染最细缩放层（zoom = MaxZoom）。
        /// </summary>
        private static bool RenderMaxZoomLevel(RegionFileManager rfm, string mapDir,
            int minChunkX, int minChunkZ, int maxChunkX, int maxChunkZ)
        {
            int minTileX = FloorDiv(minChunkX, ChunksPerTile);
            int maxTileX = FloorDiv(maxChunkX, ChunksPerTile);
            int minTileZ = FloorDiv(minChunkZ, ChunksPerTile);
            int maxTileZ = FloorDiv(maxChunkZ, ChunksPerTile);

            var rgba = new byte[TileSize * TileSize * 4];

            for (int tileX = minTileX; tileX <= maxTileX; tileX++)
            {
                for (int tileZ = minTileZ; tileZ <= maxTileZ; tileZ++)
                {
                    if (_stopRequested)
                    {
                        return false;
                    }

                    int baseChunkX = tileX * ChunksPerTile;
                    int baseChunkZ = tileZ * ChunksPerTile;

                    var (tileHasData, tileColors) = FetchTileColorsOnMainThread(rfm, baseChunkX, baseChunkZ);

                    if (_stopRequested)
                    {
                        return false;
                    }

                    if (!tileHasData)
                    {
                        continue;
                    }

                    Array.Clear(rgba, 0, rgba.Length); 
                    for (int dx = 0; dx < ChunksPerTile; dx++)
                    {
                        for (int dz = 0; dz < ChunksPerTile; dz++)
                        {
                            ushort[]? colors = tileColors[dx * ChunksPerTile + dz];
                            if (colors != null && colors.Length == ChunkPixels * ChunkPixels)
                            {
                                BlitChunkToTile(colors, rgba, dx, dz);
                            }
                        }
                    }

                    SaveTile(mapDir, MaxZoom, tileX, tileZ, rgba);

                    try { Thread.Sleep(ThrottleSleepMs); } catch { }
                }
            }

            return true;
        }


        private static (bool hasData, ushort[]?[] colors) FetchTileColorsOnMainThread(RegionFileManager rfm, int baseChunkX, int baseChunkZ)
        {
            var tileColors = new ushort[ChunksPerTile * ChunksPerTile][];
            var done = new ManualResetEventSlim(false);
            bool hasData = false;

            ModApi.MainThreadSyncContext.Post(_ =>
            {
                try
                {
                    if (GameManager.Instance?.World == null)
                    {
                        return;
                    }

                    for (int dx = 0; dx < ChunksPerTile; dx++)
                    {
                        for (int dz = 0; dz < ChunksPerTile; dz++)
                        {
                            int cx = baseChunkX + dx;
                            int cz = baseChunkZ + dz;
                            long key = WorldChunkCache.MakeChunkKey(cx, cz);

                            if (!rfm.ContainsChunkSync(key))
                            {
                                continue;
                            }

                            try
                            {
                                var chunk = rfm.GetChunkSync(key);
                                if (chunk != null)
                                {
                                    ushort[]? colors = chunk.GetMapColors();
                                    if (colors != null && colors.Length == ChunkPixels * ChunkPixels)
                                    {
                                        var snapshot = new ushort[colors.Length];
                                        Array.Copy(colors, snapshot, colors.Length);
                                        tileColors[dx * ChunksPerTile + dz] = snapshot;
                                        hasData = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                CustomLogger.Warn(ex, $"全图渲染：读取 chunk ({cx},{cz}) 颜色失败，跳过");
                            }

                            lock (_lock) { _chunksDone++; }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CustomLogger.Error(ex, "全图渲染：主线程取色任务异常");
                }
                finally
                {
                    try { done.Set(); } catch { /* 异常 */ }
                }
            }, null);

            if (!done.Wait(30000))
            {
                CustomLogger.Warn("全图渲染：主线程取色超时（30s），放弃当前瓦片");
                return (false, new ushort[ChunksPerTile * ChunksPerTile][]);
            }
            return (hasData, tileColors);
        }


        internal static void BlitChunkToTile(ushort[] colors, byte[] rgba, int chunkDx, int chunkDz)
        {
            int pxBase = chunkDx * ChunkPixels;
            int pzBase = chunkDz * ChunkPixels;

            for (int z = 0; z < ChunkPixels; z++)
            {
                for (int x = 0; x < ChunkPixels; x++)
                {
                    ushort col = colors[x + z * ChunkPixels];

                    int worldPxX = pxBase + x;
                    int worldPxZ = pzBase + z;
                    int rowFromTop = TileSize - 1 - worldPxZ;
                    int dst = (rowFromTop * TileSize + worldPxX) * 4;

                    rgba[dst] = (byte)(256 * ((col >> 10) & 31) / 32);     // R
                    rgba[dst + 1] = (byte)(256 * ((col >> 5) & 31) / 32);  // G
                    rgba[dst + 2] = (byte)(256 * (col & 31) / 32);         // B
                    rgba[dst + 3] = 255;                                   // A
                }
            }
        }

        /// <summary>
        /// 由已生成的 zoom 层（_srcZoom）逐 1/2 降采样合成上一层（_srcZoom-1）。
        /// </summary>
        private static void BuildLowerZoomLevel(string mapDir, int srcZoom, Func<bool>? shouldStop = null)
        {
            int dstZoom = srcZoom - 1;
            string srcDir = $"{mapDir}/{srcZoom}";
            if (!Directory.Exists(srcDir))
            {
                return;
            }

            var dstTiles = new System.Collections.Generic.HashSet<long>();
            foreach (string xDir in Directory.GetDirectories(srcDir))
            {
                if (!int.TryParse(Path.GetFileName(xDir), out int sx)) continue;
                foreach (string yFile in Directory.GetFiles(xDir, "*.png"))
                {
                    string name = Path.GetFileNameWithoutExtension(yFile);
                    if (!int.TryParse(name, out int sy)) continue;
                    int dx = FloorDiv(sx, 2);
                    int dy = FloorDiv(sy, 2);
                    dstTiles.Add(PackTileKey(dx, dy));
                }
            }

            var halfBuf = new byte[(TileSize / 2) * (TileSize / 2) * 4];
            var dstRgba = new byte[TileSize * TileSize * 4];

            foreach (long dstKey in dstTiles)
            {
                if (_stopRequested || (shouldStop != null && shouldStop())) return;

                UnpackTileKey(dstKey, out int dtx, out int dty);
                Array.Clear(dstRgba, 0, dstRgba.Length);
                bool any = false;

                for (int sxOff = 0; sxOff <= 1; sxOff++)
                {
                    for (int syOff = 0; syOff <= 1; syOff++)
                    {
                        int sx = dtx * 2 + sxOff;
                        int sy = dty * 2 + syOff;
                        string srcFile = $"{srcDir}/{sx}/{sy}.png";
                        if (!File.Exists(srcFile)) continue;

                        byte[] srcBytes;
                        try { srcBytes = File.ReadAllBytes(srcFile); }
                        catch { continue; }

                        if (!PurePng.TryDecode(srcBytes, out int sw, out int sh, out byte[]? srcRgba)
                            || srcRgba == null || sw != TileSize || sh != TileSize)
                        {
                            continue;
                        }

                        HalfScale(srcRgba, halfBuf);

                        int dstPxX = sxOff * (TileSize / 2);
                        int dstPxRowTop = (1 - syOff) * (TileSize / 2);

                        CopyHalfIntoTile(halfBuf, dstRgba, dstPxX, dstPxRowTop);
                        any = true;
                    }
                }

                if (any)
                {
                    SaveTile(mapDir, dstZoom, dtx, dty, dstRgba);
                }
            }
        }

        /// <summary>把 128×128 RGBA 点采样缩小为 64×64，写入 halfBuf。</summary>
        private static void HalfScale(byte[] src, byte[] half)
        {
            int hs = TileSize / 2;
            for (int row = 0; row < hs; row++)
            {
                int srcRow = row * 2;
                for (int col = 0; col < hs; col++)
                {
                    int srcCol = col * 2;
                    int srcIdx = (srcRow * TileSize + srcCol) * 4;
                    int dstIdx = (row * hs + col) * 4;
                    half[dstIdx] = src[srcIdx];
                    half[dstIdx + 1] = src[srcIdx + 1];
                    half[dstIdx + 2] = src[srcIdx + 2];
                    half[dstIdx + 3] = src[srcIdx + 3];
                }
            }
        }

        /// <summary>把 64×64 的 half 缓冲区拷入 128×128 目标瓦片的指定象限。</summary>
        private static void CopyHalfIntoTile(byte[] half, byte[] dstTile, int dstPxX, int dstPxRowTop)
        {
            int hs = TileSize / 2;
            for (int row = 0; row < hs; row++)
            {
                int dstRow = dstPxRowTop + row;
                int srcIdx = (row * hs) * 4;
                int dstIdx = (dstRow * TileSize + dstPxX) * 4;
                Buffer.BlockCopy(half, srcIdx, dstTile, dstIdx, hs * 4);
            }
        }

        /// <summary>把瓦片 RGBA 编码为 PNG 落盘。internal 供其它渲染器复用，保证输出路径/格式一致。</summary>
        internal static void SaveTile(string mapDir, int zoom, int tileX, int tileZ, byte[] rgba)
        {
            try
            {
                string dir = $"{mapDir}/{zoom}/{tileX}";
                Directory.CreateDirectory(dir);
                byte[] png = PurePng.Encode(TileSize, TileSize, rgba);
                File.WriteAllBytes($"{dir}/{tileZ}.png", png);
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, $"全图渲染：写入瓦片 z{zoom}/{tileX}/{tileZ} 失败");
            }
        }

        private static void WriteMapInfo(string mapDir)
        {
            try
            {
                string json = $"{{\"blockSize\":{TileSize},\"maxZoom\":{MaxZoom}}}";
                File.WriteAllText(mapDir + "/mapinfo.json", json);
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "全图渲染：写入 mapinfo.json 失败");
            }
        }

        /// <summary>写入 mapinfo.json。internal 供其它渲染器复用。</summary>
        internal static void WriteMapInfoFile(string mapDir) => WriteMapInfo(mapDir);

        private static void Finish(string status, Stopwatch sw, string? infoMessage, bool manuallyStopped = false)
        {
            lock (_lock)
            {
                _status = status;
                _elapsedSeconds = (float)sw.Elapsed.TotalSeconds;
                if (infoMessage != null) _error = null;
            }
            if (manuallyStopped)
            {
                CustomLogger.Info("全图渲染已被手动停止");
            }
            else if (infoMessage != null)
            {
                CustomLogger.Info($"全图渲染：{infoMessage}");
            }
        }

        /// <summary>向负无穷取整的整数除法（与 Leaflet/官方瓦片坐标对齐一致）。internal 供其它渲染器复用。</summary>
        internal static int FloorDiv(int a, int b)
        {
            int q = a / b;
            if ((a % b != 0) && ((a < 0) != (b < 0))) q--;
            return q;
        }

        /// <summary>
        /// 由最细层（MaxZoom）向上逐级 1/2 降采样
        /// </summary>
        internal static void BuildPyramidFromMaxZoom(string mapDir, Func<bool> shouldStop)
        {
            for (int z = MaxZoom; z > 0; z--)
            {
                if (shouldStop != null && shouldStop())
                {
                    return;
                }
                BuildLowerZoomLevel(mapDir, z, shouldStop);
            }
        }

        private static long PackTileKey(int x, int y)
        {
            return ((long)(uint)x << 32) | (uint)y;
        }

        private static void UnpackTileKey(long key, out int x, out int y)
        {
            x = (int)(key >> 32);
            y = (int)(key & 0xFFFFFFFF);
        }
    }
}
