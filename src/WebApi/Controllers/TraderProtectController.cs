using SdtdServerKit.Data.Entities;
using SdtdServerKit.Functions;
using SdtdServerKit.Managers;
using SdtdServerKit.TraderProtectAreas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// 自定义商人保护区域管理
    /// </summary>
    [Authorize]
    [RoutePrefix("api/TraderProtect")]
    public class TraderProtectController : ApiController
    {
        /// <summary>
        /// 获取所有商人保护区域
        /// </summary>
        [HttpGet]
        [Route("Areas")]
        [ResponseType(typeof(IEnumerable<TraderProtectAreaResult>))]
        public async Task<IHttpActionResult> GetAreas()
        {
            if (!FunctionManager.TryGetFunction<TraderProtect>(out var function))
            {
                return BadRequest("功能未加载");
            }

            // 优先返回内存快照（功能已启用），否则直接读数据库（功能未启用时也能查看）
            IEnumerable<TraderProtectAreaResult> result;
            if (TraderProtectManager.IsInitialized)
            {
                var areas = TraderProtectManager.GetAll();
                result = areas.Select(a => new TraderProtectAreaResult()
                {
                    Id = a.Id,
                    MinX = a.PosA.x,
                    MinZ = a.PosA.y,
                    MaxX = a.PosB.x,
                    MaxZ = a.PosB.y,
                    Name = a.Name,
                    CreatedAt = a.CreatedAt,
                });
            }
            else
            {
                var entities = await function!.Repository.GetAllAsync();
                result = entities.Select(e => new TraderProtectAreaResult()
                {
                    Id = e.Id,
                    MinX = e.MinX,
                    MinZ = e.MinZ,
                    MaxX = e.MaxX,
                    MaxZ = e.MaxZ,
                    Name = e.Name,
                    CreatedAt = e.CreatedAt,
                });
            }
            return Ok(result);
        }

        /// <summary>
        /// 添加商人保护区域
        /// </summary>
        [HttpPost]
        [Route("Areas")]
        public async Task<IHttpActionResult> AddArea([FromBody] AddTraderProtectAreaRequest model)
        {
            if (model == null)
            {
                return BadRequest("请求体不能为空");
            }

            if (!FunctionManager.TryGetFunction<TraderProtect>(out var function))
            {
                return BadRequest("功能未加载");
            }

            // 1. 功能已启用：经由 Manager 同步写入数据库 + 内存快照 + 注入到游戏世界
            if (TraderProtectManager.IsInitialized)
            {
                var area = await TraderProtectManager.AddAreaAsync(model.X1, model.Z1, model.X2, model.Z2, model.Name);
                return Ok(new TraderProtectAreaResult()
                {
                    Id = area.Id,
                    MinX = area.PosA.x,
                    MinZ = area.PosA.y,
                    MaxX = area.PosB.x,
                    MaxZ = area.PosB.y,
                    Name = area.Name,
                    CreatedAt = area.CreatedAt,
                });
            }

            // 2. 功能未启用：仅写入数据库（启用功能时会从数据库加载并注入到游戏世界）
            int minX = Math.Min(model.X1, model.X2);
            int maxX = Math.Max(model.X1, model.X2);
            int minZ = Math.Min(model.Z1, model.Z2);
            int maxZ = Math.Max(model.Z1, model.Z2);
            var entity = new T_TraderProtectArea
            {
                CreatedAt = DateTime.Now,
                MinX = minX,
                MinZ = minZ,
                MaxX = maxX,
                MaxZ = maxZ,
                Name = model.Name,
            };
            await function!.Repository.InsertAsync(entity);
            return Ok(new TraderProtectAreaResult()
            {
                Id = entity.Id,
                MinX = minX,
                MinZ = minZ,
                MaxX = maxX,
                MaxZ = maxZ,
                Name = model.Name,
                CreatedAt = entity.CreatedAt,
            });
        }

        /// <summary>
        /// 删除商人保护区域
        /// </summary>
        [HttpDelete]
        [Route("Areas/{id:int}")]
        public async Task<IHttpActionResult> RemoveArea(int id)
        {
            if (!FunctionManager.TryGetFunction<TraderProtect>(out var function))
            {
                return BadRequest("功能未加载");
            }

            bool success;
            if (TraderProtectManager.IsInitialized)
            {
                // 功能已启用：经由 Manager 同步移除内存快照、游戏世界、数据库
                success = await TraderProtectManager.RemoveAreaAsync(id);
            }
            else
            {
                // 功能未启用：直接从数据库删除
                int affected = await function!.Repository.DeleteByIdAsync(id);
                success = affected > 0;
            }

            if (success)
            {
                return Ok(new { message = "商人保护区域已删除" });
            }
            return NotFound();
        }

        /// <summary>
        /// 清空所有商人保护区域
        /// </summary>
        [HttpDelete]
        [Route("Areas/All")]
        public async Task<IHttpActionResult> ClearAllAreas()
        {
            if (!FunctionManager.TryGetFunction<TraderProtect>(out var function))
            {
                return BadRequest("功能未加载");
            }

            if (TraderProtectManager.IsInitialized)
            {
                await TraderProtectManager.ClearAllAsync();
            }
            else
            {
                await function!.Repository.DeleteAllAsync();
            }
            return Ok(new { message = "所有商人保护区域已清空" });
        }
    }

    /// <summary>
    /// 添加商人保护区域请求
    /// </summary>
    public class AddTraderProtectAreaRequest
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

        /// <summary>
        /// 区域备注名称（可选）
        /// </summary>
        public string? Name { get; set; }
    }

    /// <summary>
    /// 商人保护区域结果
    /// </summary>
    public class TraderProtectAreaResult
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
        /// 区域备注名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
