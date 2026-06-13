using SdtdServerKit.Functions;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;
using System.Collections.Generic;
using System.Linq;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// 定时区域重置
    /// </summary>
    [Authorize]
    [RoutePrefix("api/ChunkReset")]
    public class ChunkResetController : ApiController
    {
        /// <summary>
        /// 获取所有重置区域
        /// </summary>
        [HttpGet]
        [Route("Areas")]
        [ResponseType(typeof(IEnumerable<ChunkResetAreaResult>))]
        public IHttpActionResult GetAreas()
        {
            if (!FunctionManager.TryGetFunction<ChunkReset>(out var function))
            {
                return BadRequest("功能未加载");
            }

            var areas = function!.GetAreas();
            var result = areas.Select(a => new ChunkResetAreaResult()
            {
                Id = a.Id,
                MinX = a.MinX,
                MinZ = a.MinZ,
                MaxX = a.MaxX,
                MaxZ = a.MaxZ,
                CreatedAt = a.CreatedAt
            });
            return Ok(result);
        }

        /// <summary>
        /// 获取重置状态信息
        /// </summary>
        [HttpGet]
        [Route("Status")]
        [ResponseType(typeof(ChunkResetStatus))]
        public IHttpActionResult GetStatus()
        {
            if (!FunctionManager.TryGetFunction<ChunkReset>(out var function))
            {
                return BadRequest("功能未加载");
            }

            var status = new ChunkResetStatus()
            {
                IsEnabled = function!.Settings.IsEnabled,
                Interval = function.Settings.Interval,
                LastResetTime = function.LastResetTime,
                NextResetTime = function.NextResetTime,
                AreaCount = function.GetAreas().Count
            };
            return Ok(status);
        }

        /// <summary>
        /// 添加重置区域
        /// </summary>
        [HttpPost]
        [Route("Areas")]
        public async System.Threading.Tasks.Task<IHttpActionResult> AddArea([FromBody] AddChunkResetAreaRequest model)
        {
            if (model == null)
            {
                return BadRequest("请求体不能为空");
            }

            if (!FunctionManager.TryGetFunction<ChunkReset>(out var function))
            {
                return BadRequest("功能未加载");
            }

            var area = await function!.AddAreaAsync(model.X1, model.Z1, model.X2, model.Z2);
            return Ok(new ChunkResetAreaResult()
            {
                Id = area.Id,
                MinX = area.MinX,
                MinZ = area.MinZ,
                MaxX = area.MaxX,
                MaxZ = area.MaxZ,
                CreatedAt = area.CreatedAt
            });
        }

        /// <summary>
        /// 删除重置区域
        /// </summary>
        [HttpDelete]
        [Route("Areas/{id:int}")]
        public async System.Threading.Tasks.Task<IHttpActionResult> RemoveArea(int id)
        {
            if (!FunctionManager.TryGetFunction<ChunkReset>(out var function))
            {
                return BadRequest("功能未加载");
            }

            bool success = await function!.RemoveAreaAsync(id);
            if (success)
            {
                return Ok(new { message = "重置区域已删除" });
            }
            return NotFound();
        }

        /// <summary>
        /// 清空所有重置区域
        /// </summary>
        [HttpDelete]
        [Route("Areas/All")]
        public async System.Threading.Tasks.Task<IHttpActionResult> ClearAllAreas()
        {
            if (!FunctionManager.TryGetFunction<ChunkReset>(out var function))
            {
                return BadRequest("功能未加载");
            }

            await function!.ClearAllAreasAsync();
            return Ok(new { message = "所有重置区域已清空" });
        }

        /// <summary>
        /// 立即重置所有区域
        /// </summary>
        [HttpPost]
        [Route("ResetNow")]
        public IHttpActionResult ResetNow()
        {
            if (!FunctionManager.TryGetFunction<ChunkReset>(out var function))
            {
                return BadRequest("功能未加载");
            }

            function!.ResetAllNow();
            return Ok(new { message = "已触发立即重置" });
        }

        /// <summary>
        /// 重设下次重置时间
        /// </summary>
        [HttpPost]
        [Route("ResetNextTime")]
        public IHttpActionResult ResetNextTime()
        {
            if (!FunctionManager.TryGetFunction<ChunkReset>(out var function))
            {
                return BadRequest("功能未加载");
            }

            function!.ResetNextTime();
            return Ok(new { message = "已重设下次重置时间" });
        }
    }

    /// <summary>
    /// 添加重置区域请求
    /// </summary>
    public class AddChunkResetAreaRequest
    {
        /// <summary>
        /// 区域顶角X坐标
        /// </summary>
        public int X1 { get; set; }

        /// <summary>
        /// 区域顶角Z坐标
        /// </summary>
        public int Z1 { get; set; }

        /// <summary>
        /// 区域对角X坐标
        /// </summary>
        public int X2 { get; set; }

        /// <summary>
        /// 区域对角Z坐标
        /// </summary>
        public int Z2 { get; set; }
    }

    /// <summary>
    /// 重置区域结果
    /// </summary>
    public class ChunkResetAreaResult
    {
        /// <summary>
        /// 唯一Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 区域最小X坐标
        /// </summary>
        public int MinX { get; set; }

        /// <summary>
        /// 区域最小Z坐标
        /// </summary>
        public int MinZ { get; set; }

        /// <summary>
        /// 区域最大X坐标
        /// </summary>
        public int MaxX { get; set; }

        /// <summary>
        /// 区域最大Z坐标
        /// </summary>
        public int MaxZ { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 区域重置状态
    /// </summary>
    public class ChunkResetStatus
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 重置间隔（秒）
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// 上次重置时间
        /// </summary>
        public DateTime LastResetTime { get; set; }

        /// <summary>
        /// 下次重置时间
        /// </summary>
        public DateTime NextResetTime { get; set; }

        /// <summary>
        /// 区域数量
        /// </summary>
        public int AreaCount { get; set; }
    }
}
