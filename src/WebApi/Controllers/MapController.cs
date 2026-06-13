using SdtdServerKit.Functions;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// 地图
    /// </summary>
    // [Authorize]
    [RoutePrefix("api/Map")]
    public class MapController : ApiController
    {

        /// <summary>
        /// 获取地图信息
        /// </summary>
        [HttpGet]
        [Route("Info")]
        public MapInfo MapInfo()
        {
            var mapInfo = new MapInfo()
            {
                BlockSize = MapTileRenderer.TileSize,
                MaxZoom = MapTileRenderer.ZoomLevels - 1
            };
            return mapInfo;
        }

        /// <summary>
        /// 获取切片。直接从存档 map 目录读取
        /// </summary>
        [HttpGet]
        [Route("Tile/{z:int}/{x:int}/{y:int}")]
        public IHttpActionResult MapTile(int z, int x, int y)
        {
            string fileName = GameIO.GetSaveGameDir() + $"/map/{z}/{x}/{y}.png";

            if (File.Exists(fileName))
            {
                return new FileStreamResult(File.OpenRead(fileName), "image/png");
            }

            return NotFound();
        }

        /// <summary>
        /// 渲染完整地图
        /// </summary>
        [HttpPost]
        [Route("RenderFullMap")]
        public IHttpActionResult RenderFullMap()
        {
            try
            {
                if (FullMapGenerateRenderer.IsRunning)
                {
                    var p = FullMapGenerateRenderer.GetProgress();
                    return Ok(new
                    {
                        success = false,
                        message = "完整地图渲染正在进行中，请勿重复触发",
                        status = p.status,
                        chunksDone = p.chunksDone,
                        chunksTotal = p.chunksTotal
                    });
                }

                if (MapTileRenderer.IsRunning)
                {
                    return Ok(new { success = false, message = "已探索区域渲染正在进行中，请等待其完成后再渲染完整地图" });
                }

                if (MapImageExporter.IsRunning)
                {
                    return Ok(new { success = false, message = "地图图片导出正在进行中，请等待其完成后再渲染完整地图" });
                }

                ModApi.MainThreadSyncContext.Post(_ =>
                {
                    try
                    {
                        if (!FullMapGenerateRenderer.Start())
                        {
                            CustomLogger.Debug("完整地图渲染：已有渲染任务在进行或前置校验失败，忽略本次请求");
                        }
                    }
                    catch (Exception e)
                    {
                        CustomLogger.Debug(e, "完整地图渲染启动失败");
                    }
                }, null);

                return Ok(new { success = true, message = "完整地图渲染已提交，请通过 RenderFullMapStatus 查询进度" });
            }
            catch (Exception e)
            {
                CustomLogger.Error(e, "完整地图渲染请求异常");
                return InternalServerError(e);
            }
        }

        /// <summary>
        /// 查询完整地图渲染进度
        /// </summary>
        [HttpGet]
        [Route("RenderFullMapStatus")]
        public IHttpActionResult RenderFullMapStatus()
        {
            var p = FullMapGenerateRenderer.GetProgress();
            int percent = p.chunksTotal > 0
                ? (int)(100f * p.chunksDone / p.chunksTotal)
                : 0;

            return Ok(new
            {
                status = p.status,
                chunksDone = p.chunksDone,
                chunksTotal = p.chunksTotal,
                percent = percent,
                elapsedSeconds = Math.Round(p.elapsedSeconds, 1),
                error = p.error
            });
        }

        /// <summary>
        /// 停止完整地图渲染
        /// </summary>
        [HttpPost]
        [Route("StopRenderFullMap")]
        public IHttpActionResult StopRenderFullMap()
        {
            if (!FullMapGenerateRenderer.IsRunning)
            {
                return Ok(new { success = false, message = "当前没有正在进行的完整地图渲染" });
            }

            FullMapGenerateRenderer.Stop();

            return Ok(new { success = true, message = "已发送停止指令" });
        }

        /// <summary>
        /// 渲染已探索区域
        /// </summary>
        [HttpPost]
        [Route("RenderExploredArea")]
        public IHttpActionResult RenderExploredArea()
        {
            try
            {
                if (MapTileRenderer.IsRunning)
                {
                    return Ok(new { success = false, message = "已探索区域渲染正在进行中，请勿重复触发" });
                }
                if (FullMapGenerateRenderer.IsRunning)
                {
                    return Ok(new { success = false, message = "完整地图渲染正在进行中，请等待其完成后再渲染已探索区域" });
                }

                ModApi.MainThreadSyncContext.Post(_ =>
                {
                    try
                    {
                        var world = GameManager.Instance?.World;
                        if (world == null)
                        {
                            CustomLogger.Error("已探索区域渲染失败：World 尚未初始化");
                            return;
                        }

                        string saveGameDir = GameIO.GetSaveGameDir();
                        string regionDir = GameIO.GetSaveGameRegionDir();

                        if (!MapTileRenderer.Start(saveGameDir, regionDir))
                        {
                            CustomLogger.Warn("已探索区域渲染：已有渲染任务在进行，忽略本次请求");
                        }
                    }
                    catch (Exception e)
                    {
                        CustomLogger.Error(e, "已探索区域渲染启动失败");
                    }
                }, null);

                return Ok(new { success = true, message = "已探索区域渲染已提交" });
            }
            catch (Exception e)
            {
                CustomLogger.Error(e, "已探索区域渲染请求异常");
                return InternalServerError(e);
            }
        }

        /// <summary>
        /// 查询已探索区域渲染进度
        /// </summary>
        [HttpGet]
        [Route("RenderExploredAreaStatus")]
        public IHttpActionResult RenderExploredAreaStatus()
        {
            var p = MapTileRenderer.GetProgress();
            int percent = p.chunksTotal > 0
                ? (int)(100f * p.chunksDone / p.chunksTotal)
                : 0;

            return Ok(new
            {
                status = p.status,
                chunksDone = p.chunksDone,
                chunksTotal = p.chunksTotal,
                percent = percent,
                elapsedSeconds = Math.Round(p.elapsedSeconds, 1),
                error = p.error
            });
        }

        /// <summary>
        /// 停止已探索区域渲染
        /// </summary>
        [HttpPost]
        [Route("StopRenderExploredArea")]
        public IHttpActionResult StopRenderExploredArea()
        {
            if (!MapTileRenderer.IsRunning)
            {
                return Ok(new { success = false, message = "当前没有正在进行的已探索区域渲染" });
            }
            MapTileRenderer.Stop();
            return Ok(new { success = true, message = "已发送停止指令" });
        }

        /// <summary>
        /// 导出整张地图
        /// </summary>
        [HttpPost]
        [Route("ExportMapImage")]
        public IHttpActionResult ExportMapImage()
        {
            try
            {
                if (MapImageExporter.IsRunning)
                {
                    var ep = MapImageExporter.GetProgress();
                    return Ok(new
                    {
                        success = false,
                        message = "地图图片导出正在进行中，请勿重复触发",
                        status = ep.status,
                        chunksDone = ep.chunksDone,
                        chunksTotal = ep.chunksTotal
                    });
                }

                if (MapTileRenderer.IsRunning || FullMapGenerateRenderer.IsRunning)
                {
                    return Ok(new { success = false, message = "地图渲染正在进行中，请等待其完成后再导出图片" });
                }

                string mapDir = GameIO.GetSaveGameDir() + "/map";
                if (!Directory.Exists(mapDir))
                {
                    return Ok(new { success = false, message = "尚未生成地图瓦片，请先执行“完整地图渲染”或“渲染已探索区域”，待其完成后再导出图片" });
                }

                string exportDir = Path.Combine(AppContext.BaseDirectory, "LSTY_Data", "MapExport");

                if (!MapImageExporter.Start(mapDir, exportDir))
                {
                    return Ok(new { success = false, message = "地图图片导出正在进行中，请勿重复触发" });
                }

                return Ok(new { success = true, message = "地图图片导出已提交，请通过 ExportMapImageStatus 查询进度" });
            }
            catch (Exception e)
            {
                CustomLogger.Error(e, "地图图片导出请求异常");
                return InternalServerError(e);
            }
        }

        /// <summary>
        /// 查询地图图片导出进度
        /// </summary>
        [HttpGet]
        [Route("ExportMapImageStatus")]
        public IHttpActionResult ExportMapImageStatus()
        {
            var p = MapImageExporter.GetProgress();
            int percent = p.chunksTotal > 0
                ? (int)(100f * p.chunksDone / p.chunksTotal)
                : 0;

            return Ok(new
            {
                status = p.status,
                chunksDone = p.chunksDone,
                chunksTotal = p.chunksTotal,
                percent = percent,
                elapsedSeconds = Math.Round(p.elapsedSeconds, 1),
                error = p.error,
                outputFile = p.outputFile != null ? Path.GetFileName(p.outputFile) : null
            });
        }

        /// <summary>
        /// 停止地图图片导出
        /// </summary>
        [HttpPost]
        [Route("StopExportMapImage")]
        public IHttpActionResult StopExportMapImage()
        {
            if (!MapImageExporter.IsRunning)
            {
                return Ok(new { success = false, message = "当前没有正在进行的地图图片导出" });
            }
            MapImageExporter.Stop();
            return Ok(new { success = true, message = "已发送停止指令" });
        }

        /// <summary>
        /// 获取所有 POI 预制件
        /// </summary>
        [HttpGet]
        [Route("Prefabs")]
        [ResponseType(typeof(List<PrefabInfo>))]
        public IHttpActionResult GetPrefabs()
        {
            try
            {
                var decorator = GameManager.Instance?.GetDynamicPrefabDecorator();
                if (decorator == null)
                {
                    return Ok(Array.Empty<object>());
                }

                var prefabs = decorator.GetDynamicPrefabs();
                var result = new List<PrefabInfo>(prefabs.Count);

                foreach (var pi in prefabs)
                {
                    if (pi == null) continue;
                    if (pi.boundingBoxSize.x <= 0 || pi.boundingBoxSize.z <= 0) continue;
                    result.Add(new PrefabInfo
                    {
                        Id = pi.id,
                        Name = pi.location.Name,
                        X = pi.boundingBoxPosition.x,
                        Y = pi.boundingBoxPosition.y,
                        Z = pi.boundingBoxPosition.z,
                        SizeX = pi.boundingBoxSize.x,
                        SizeY = pi.boundingBoxSize.y,
                        SizeZ = pi.boundingBoxSize.z,
                        IsTrader = pi.prefab != null && pi.prefab.bTraderArea,
                        Rotation = pi.rotation,
                    });
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                CustomLogger.Error(e, "获取 Prefab 列表异常");
                return InternalServerError(e);
            }
        }
    }
}
