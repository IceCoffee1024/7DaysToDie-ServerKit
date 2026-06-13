using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using SdtdServerKit.Utilities;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// 导出整张世界地图
    /// </summary>
    internal static class MapImageExporter
    {
        private const int TileSize = MapTileRenderer.TileSize;

        private const int MaxZoom = MapTileRenderer.MaxZoom;

        private const int MaxImageEdge = 16384;
        private const long MaxImagePixels = 16384L * 16384L;

        private static readonly object _lock = new object();
        private static Thread? _worker;
        private static volatile bool _stopRequested;

        private static string _status = "idle";
        private static int _chunksDone;
        private static int _chunksTotal;
        private static float _elapsedSeconds;
        private static string? _error;
        private static string? _outputFile;

        private const int ThrottleSleepMs = 2;

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
        /// 启动后台地图图片导出
        /// </summary>
        public static bool Start(string mapDir, string exportDir)
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
                _outputFile = null;

                _worker = new Thread(() => ExportLoop(mapDir, exportDir))
                {
                    Name = "TianYiServerKit_MapImageExporter",
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

        public static (string status, int chunksDone, int chunksTotal, float elapsedSeconds, string? error, string? outputFile) GetProgress()
        {
            lock (_lock)
            {
                return (_status, _chunksDone, _chunksTotal, _elapsedSeconds, _error, _outputFile);
            }
        }

        private static void ExportLoop(string mapDir, string exportDir)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                string maxZoomDir = $"{mapDir}/{MaxZoom}";
                if (!Directory.Exists(maxZoomDir))
                {
                    Fail(sw, "尚未生成地图瓦片，无法导出。请先执行“完整地图渲染”或“渲染已探索区域”，待其完成后再导出图片。");
                    return;
                }

                var tiles = new List<(int tx, int tz, string path)>();
                int minTileX = int.MaxValue, maxTileX = int.MinValue;
                int minTileZ = int.MaxValue, maxTileZ = int.MinValue;

                foreach (string xDir in Directory.GetDirectories(maxZoomDir))
                {
                    if (!int.TryParse(Path.GetFileName(xDir), out int tx)) continue;
                    foreach (string yFile in Directory.GetFiles(xDir, "*.png"))
                    {
                        if (!int.TryParse(Path.GetFileNameWithoutExtension(yFile), out int tz)) continue;
                        tiles.Add((tx, tz, yFile));
                        if (tx < minTileX) minTileX = tx;
                        if (tx > maxTileX) maxTileX = tx;
                        if (tz < minTileZ) minTileZ = tz;
                        if (tz > maxTileZ) maxTileZ = tz;
                    }
                }

                if (tiles.Count == 0)
                {
                    Fail(sw, "地图瓦片为空，无法导出。请先渲染地图后再导出图片。");
                    return;
                }

                int widthTiles = maxTileX - minTileX + 1;
                int heightTiles = maxTileZ - minTileZ + 1;
                long widthPx = (long)widthTiles * TileSize;
                long heightPx = (long)heightTiles * TileSize;

                if (widthPx > MaxImageEdge || heightPx > MaxImageEdge || widthPx * heightPx > MaxImagePixels)
                {
                    Fail(sw, $"地图过大（{widthPx}×{heightPx} 像素），超过单张图片上限（{MaxImageEdge}×{MaxImageEdge}）。" +
                        "请改用网页瓦片地图浏览，或缩小渲染范围。");
                    return;
                }

                lock (_lock) { _chunksTotal = tiles.Count; }

                CustomLogger.Debug($"地图图片导出已启动：瓦片范围 X[{minTileX},{maxTileX}] Z[{minTileZ},{maxTileZ}]，" +
                    $"图片尺寸 {widthPx}×{heightPx} 像素，共 {tiles.Count} 张瓦片");

                int W = (int)widthPx;
                int H = (int)heightPx;
                long byteLen = (long)W * H * 4;
                if (byteLen > int.MaxValue)
                {
                    Fail(sw, "图片过大，超过单个数组容量上限");
                    return;
                }
                var rgba = new byte[(int)byteLen];

                // 逐瓦片解码并拼入大图。
                foreach (var (tx, tz, path) in tiles)
                {
                    if (_stopRequested)
                    {
                        Finish("idle", sw, manuallyStopped: true);
                        return;
                    }

                    try
                    {
                        byte[] bytes = File.ReadAllBytes(path);
                        if (PurePng.TryDecode(bytes, out int tw, out int th, out byte[]? tileRgba)
                            && tileRgba != null && tw == TileSize && th == TileSize)
                        {
                            int colPx = (tx - minTileX) * TileSize;
                            int rowTopPx = (maxTileZ - tz) * TileSize;
                            BlitTile(tileRgba, rgba, W, H, colPx, rowTopPx);
                        }
                        else
                        {
                            CustomLogger.Debug($"地图图片导出：瓦片 {path} 解码失败或尺寸异常，跳过");
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.Debug(ex, $"地图图片导出：读取瓦片 {path} 失败，跳过");
                    }

                    lock (_lock) { _chunksDone++; }
                    try { Thread.Sleep(ThrottleSleepMs); } catch { }
                }

                if (_stopRequested)
                {
                    Finish("idle", sw, manuallyStopped: true);
                    return;
                }

                // 编码并写盘
                Directory.CreateDirectory(exportDir);
                string fileName = $"map_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string fullPath = Path.Combine(exportDir, fileName);
                byte[] png = PurePng.Encode(W, H, rgba);
                File.WriteAllBytes(fullPath, png);

                lock (_lock)
                {
                    _outputFile = fullPath;
                    _status = "done";
                    _elapsedSeconds = (float)sw.Elapsed.TotalSeconds;
                }
                CustomLogger.Info($"地图图片导出完成：{fullPath}（{W}×{H} 像素，{png.Length / 1024}KB，耗时 {_elapsedSeconds:F1} 秒）");
            }
            catch (Exception ex)
            {
                Fail(sw, ex.Message);
                CustomLogger.Error(ex, "地图图片导出异常");
            }
        }

        /// <summary>
        /// 把一张 128×128 的瓦片 RGBA 整块拷入大图。
        /// </summary>
        private static void BlitTile(byte[] tileRgba, byte[] dst, int imgW, int imgH, int colPx, int rowTopPx)
        {
            int rowBytes = TileSize * 4;
            for (int row = 0; row < TileSize; row++)
            {
                int dstRow = rowTopPx + row;
                if (dstRow < 0 || dstRow >= imgH) continue;
                if (colPx < 0 || colPx + TileSize > imgW) continue;
                int dstIdx = (dstRow * imgW + colPx) * 4;
                int srcIdx = row * rowBytes;
                Buffer.BlockCopy(tileRgba, srcIdx, dst, dstIdx, rowBytes);
            }
        }

        private static void Fail(Stopwatch sw, string message)
        {
            lock (_lock)
            {
                _status = "error";
                _error = message;
                _elapsedSeconds = (float)sw.Elapsed.TotalSeconds;
            }
            CustomLogger.Debug($"地图图片导出失败：{message}");
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
                CustomLogger.Info("地图图片导出已被手动停止");
            }
        }
    }
}
