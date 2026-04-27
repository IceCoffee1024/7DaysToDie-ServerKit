using MapRendering;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// 地图
    /// </summary>
    // [Authorize]
    [RoutePrefix("api/Map")]
    public class MapController : ApiController
    {
        private static MapVisitor? _currentVisitor;
        private static readonly object _visitLock = new object();
        private static int _visitChunksDone;
        private static int _visitChunksTotal;
        private static float _visitElapsed;
        private static string _visitStatus = "idle";
        private static string? _visitError;

        /// <summary>
        /// 获取地图信息
        /// </summary>
        [HttpGet]
        [Route("Info")]
        public MapInfo MapInfo()
        {
            var mapInfo = new MapInfo()
            {
                BlockSize = MapRendering.Constants.MapBlockSize,
                MaxZoom = MapRendering.Constants.Zoomlevels - 1
            };
            return mapInfo;
        }

        /// <summary>
        /// 获取切片
        /// </summary>
        [HttpGet]
        [Route("Tile/{z:int}/{x:int}/{y:int}")]
        public IHttpActionResult MapTile(int z, int x, int y)
        {
            string fileName = MapRendering.Constants.MapDirectory + $"/{z}/{x}/{y}.png";

            if (File.Exists(fileName))
            {
                return new FileStreamResult(File.OpenRead(fileName), "image/png");
            }

            if (ModApi.MapTileCache == null)
            {
                return NotFound();
            }

            byte[] data = ModApi.MapTileCache.GetFileContent(fileName);
            return new FileContentResult(data, "image/png");
        }

        /// <summary>
        /// 渲染整个地图
        /// </summary>
        [HttpPost]
        [Route("RenderFullMap")]
        public IHttpActionResult RenderFullMap()
        {
            lock (_visitLock)
            {
                if (_visitStatus == "running" && _currentVisitor != null && _currentVisitor.IsRunning())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "全图渲染正在进行中，请勿重复触发",
                        status = _visitStatus,
                        chunksDone = _visitChunksDone,
                        chunksTotal = _visitChunksTotal
                    });
                }
            }

            try
            {
                ModApi.MainThreadSyncContext.Post(_ =>
                {
                    try
                    {
                        var world = GameManager.Instance?.World;
                        if (world == null)
                        {
                            lock (_visitLock)
                            {
                                _visitStatus = "error";
                                _visitError = "World 尚未初始化";
                            }
                            Log.Error("[ServerKit] 全图渲染失败：World 尚未初始化");
                            return;
                        }

                        Vector3i minPos, maxPos;
                        world.GetWorldExtent(out minPos, out maxPos);

                        var visitor = new MapVisitor(minPos, maxPos);

                        lock (_visitLock)
                        {
                            _currentVisitor = visitor;
                            _visitChunksDone = 0;
                            _visitChunksTotal = 0;
                            _visitElapsed = 0f;
                            _visitStatus = "running";
                            _visitError = null;
                        }

                        visitor.OnVisitChunk += (chunk, done, total, elapsed) =>
                        {
                            chunk.GetMapColors();

                            lock (_visitLock)
                            {
                                _visitChunksDone = done;
                                _visitChunksTotal = total;
                                _visitElapsed = elapsed;
                            }

                            if (done % 500 == 0)
                            {
                                int pct = total > 0 ? (int)(100f * done / total) : 0;
                                Log.Out($"[ServerKit] 全图渲染进度：{done}/{total} ({pct}%)");
                            }
                        };

                        visitor.OnVisitMapDone += (chunks, duration) =>
                        {
                            lock (_visitLock)
                            {
                                _visitChunksDone = chunks;
                                _visitElapsed = duration;
                                _visitStatus = "done";
                                _currentVisitor = null;
                            }
                            Log.Out($"[ServerKit] 全图渲染完成，共 {chunks} 个 Chunk，耗时 {duration:F1} 秒");
                        };

                        visitor.Start();
                        Log.Out($"[ServerKit] 全图渲染已启动，范围：{visitor.WorldPosStart} ~ {visitor.WorldPosEnd}");
                    }
                    catch (Exception e)
                    {
                        lock (_visitLock)
                        {
                            _visitStatus = "error";
                            _visitError = e.Message;
                            _currentVisitor = null;
                        }
                        Log.Error($"[ServerKit] 全图渲染启动失败：{e}");
                    }
                }, null);

                return Ok(new { success = true, message = "全图渲染已提交，请通过 RenderFullMapStatus 查询进度" });
            }
            catch (Exception e)
            {
                Log.Error($"[ServerKit] 全图渲染请求异常：{e}");
                return InternalServerError(e);
            }
        }

        /// <summary>
        /// 查询全图渲染进度
        /// </summary>
        [HttpGet]
        [Route("RenderFullMapStatus")]
        public IHttpActionResult RenderFullMapStatus()
        {
            lock (_visitLock)
            {
                int percent = _visitChunksTotal > 0
                    ? (int)(100f * _visitChunksDone / _visitChunksTotal)
                    : 0;

                return Ok(new
                {
                    status = _visitStatus,
                    chunksDone = _visitChunksDone,
                    chunksTotal = _visitChunksTotal,
                    percent = percent,
                    elapsedSeconds = Math.Round(_visitElapsed, 1),
                    error = _visitError
                });
            }
        }

        /// <summary>
        /// 停止全图渲染
        /// </summary>
        [HttpPost]
        [Route("StopRenderFullMap")]
        public IHttpActionResult StopRenderFullMap()
        {
            lock (_visitLock)
            {
                if (_currentVisitor == null || _visitStatus != "running")
                {
                    return Ok(new { success = false, message = "当前没有正在进行的全图渲染" });
                }
            }

            ModApi.MainThreadSyncContext.Post(_ =>
            {
                try
                {
                    lock (_visitLock)
                    {
                        if (_currentVisitor != null)
                        {
                            _currentVisitor.Stop();
                            _currentVisitor = null;
                            _visitStatus = "idle";
                            Log.Out("[ServerKit] 全图渲染已手动停止");
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"[ServerKit] 停止全图渲染异常：{e}");
                }
            }, null);

            return Ok(new { success = true, message = "已发送停止指令" });
        }

        /// <summary>
        /// 渲染已探索区域
        /// </summary>
        [HttpPost]
        [Route("RenderExploredArea")]
        public IHttpActionResult RenderExploredArea()
        {
            ModApi.MainThreadSyncContext.Post((state) =>
            {
                MapRenderer.Instance.RenderFullMap();
            }, null);

            return Ok();
        }
    }
}
