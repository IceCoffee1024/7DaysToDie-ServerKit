using SdtdServerKit.Data.Entities;
using SdtdServerKit.Functions;
using SdtdServerKit.Managers;
using SdtdServerKit.PvpVeAreas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// PVP/PVE 混合区域管理
    /// </summary>
    [Authorize]
    [RoutePrefix("api/PvpVe")]
    public class PvpVeController : ApiController
    {
        /// <summary>
        /// 获取所有 PVP/PVE 混合区域
        /// </summary>
        [HttpGet]
        [Route("Areas")]
        [ResponseType(typeof(IEnumerable<PvpVeAreaResult>))]
        public async Task<IHttpActionResult> GetAreas()
        {
            if (!FunctionManager.TryGetFunction<PvpVe>(out var function))
            {
                return BadRequest("功能未加载");
            }

            IEnumerable<PvpVeAreaResult> result;
            if (PvpVeManager.IsInitialized)
            {
                var areas = PvpVeManager.GetAll();
                result = areas.Select(a => new PvpVeAreaResult()
                {
                    Id = a.Id,
                    MinX = a.PosA.x,
                    MinZ = a.PosA.y,
                    MaxX = a.PosB.x,
                    MaxZ = a.PosB.y,
                    KillMode = a.KillMode,
                    DropOnDeath = a.DropOnDeath,
                    LandClaimOnline = a.LandClaimOnline,
                    LandClaimOffline = a.LandClaimOffline,
                    BuffName = a.BuffName,
                    Name = a.Name,
                    CreatedAt = a.CreatedAt,
                });
            }
            else
            {
                var entities = await function!.Repository.GetAllAsync();
                result = entities.Select(e => new PvpVeAreaResult()
                {
                    Id = e.Id,
                    MinX = e.MinX,
                    MinZ = e.MinZ,
                    MaxX = e.MaxX,
                    MaxZ = e.MaxZ,
                    KillMode = e.KillMode,
                    DropOnDeath = e.DropOnDeath,
                    LandClaimOnline = e.LandClaimOnline,
                    LandClaimOffline = e.LandClaimOffline,
                    BuffName = e.BuffName ?? string.Empty,
                    Name = e.Name,
                    CreatedAt = e.CreatedAt,
                });
            }
            return Ok(result);
        }

        /// <summary>
        /// 添加 PVP/PVE 混合区域
        /// </summary>
        [HttpPost]
        [Route("Areas")]
        public async Task<IHttpActionResult> AddArea([FromBody] AddPvpVeAreaRequest model)
        {
            if (model == null)
            {
                return BadRequest("请求体不能为空");
            }

            if (!FunctionManager.TryGetFunction<PvpVe>(out var function))
            {
                return BadRequest("功能未加载");
            }

            // 1. 功能已启用：经由 Manager 同步写入数据库 + 内存快照
            if (PvpVeManager.IsInitialized)
            {
                var area = await PvpVeManager.AddAreaAsync(
                    model.X1, model.Z1, model.X2, model.Z2,
                    model.KillMode, model.DropOnDeath,
                    model.LandClaimOnline, model.LandClaimOffline,
                    model.BuffName ?? string.Empty, model.Name);
                function!.ForceRefresh();
                return Ok(new PvpVeAreaResult()
                {
                    Id = area.Id,
                    MinX = area.PosA.x,
                    MinZ = area.PosA.y,
                    MaxX = area.PosB.x,
                    MaxZ = area.PosB.y,
                    KillMode = area.KillMode,
                    DropOnDeath = area.DropOnDeath,
                    LandClaimOnline = area.LandClaimOnline,
                    LandClaimOffline = area.LandClaimOffline,
                    BuffName = area.BuffName,
                    Name = area.Name,
                    CreatedAt = area.CreatedAt,
                });
            }

            // 2. 功能未启用：仅写入数据库
            int minX = Math.Min(model.X1, model.X2);
            int maxX = Math.Max(model.X1, model.X2);
            int minZ = Math.Min(model.Z1, model.Z2);
            int maxZ = Math.Max(model.Z1, model.Z2);
            var entity = new T_PvpVeArea
            {
                CreatedAt = DateTime.Now,
                MinX = minX,
                MinZ = minZ,
                MaxX = maxX,
                MaxZ = maxZ,
                KillMode = ClampInt(model.KillMode, 0, 3),
                DropOnDeath = ClampInt(model.DropOnDeath, 0, 3),
                LandClaimOnline = Math.Max(0, model.LandClaimOnline),
                LandClaimOffline = Math.Max(0, model.LandClaimOffline),
                BuffName = model.BuffName ?? string.Empty,
                Name = model.Name,
            };
            await function!.Repository.InsertAsync(entity);
            return Ok(new PvpVeAreaResult()
            {
                Id = entity.Id,
                MinX = minX,
                MinZ = minZ,
                MaxX = maxX,
                MaxZ = maxZ,
                KillMode = entity.KillMode,
                DropOnDeath = entity.DropOnDeath,
                LandClaimOnline = entity.LandClaimOnline,
                LandClaimOffline = entity.LandClaimOffline,
                BuffName = entity.BuffName,
                Name = entity.Name,
                CreatedAt = entity.CreatedAt,
            });
        }

        /// <summary>
        /// 删除 PVP/PVE 混合区域
        /// </summary>
        [HttpDelete]
        [Route("Areas/{id:int}")]
        public async Task<IHttpActionResult> RemoveArea(int id)
        {
            if (!FunctionManager.TryGetFunction<PvpVe>(out var function))
            {
                return BadRequest("功能未加载");
            }

            bool success;
            if (PvpVeManager.IsInitialized)
            {
                success = await PvpVeManager.RemoveAreaAsync(id);
                function!.ForceRefresh();
            }
            else
            {
                int affected = await function!.Repository.DeleteByIdAsync(id);
                success = affected > 0;
            }

            if (success)
            {
                return Ok(new { message = "PVP/PVE 混合区域已删除" });
            }
            return NotFound();
        }

        /// <summary>
        /// 清空所有 PVP/PVE 混合区域
        /// </summary>
        [HttpDelete]
        [Route("Areas/All")]
        public async Task<IHttpActionResult> ClearAllAreas()
        {
            if (!FunctionManager.TryGetFunction<PvpVe>(out var function))
            {
                return BadRequest("功能未加载");
            }

            if (PvpVeManager.IsInitialized)
            {
                await PvpVeManager.ClearAllAsync();
                function!.ForceRefresh();
            }
            else
            {
                await function!.Repository.DeleteAllAsync();
            }
            return Ok(new { message = "所有 PVP/PVE 混合区域已清空" });
        }

        private static int ClampInt(int v, int min, int max) => Math.Max(min, Math.Min(max, v));
    }

    /// <summary>
    /// 添加 PVP/PVE 混合区域请求
    /// </summary>
    public class AddPvpVeAreaRequest
    {
        /// <summary>区域顶角X坐标</summary>
        public int X1 { get; set; }
        /// <summary>区域顶角Z坐标</summary>
        public int Z1 { get; set; }
        /// <summary>区域对角X坐标</summary>
        public int X2 { get; set; }
        /// <summary>区域对角Z坐标</summary>
        public int Z2 { get; set; }
        /// <summary>杀戮模式: 0=无伤害, 1=队友伤害, 2=陌生人伤害, 3=所有人伤害</summary>
        public int KillMode { get; set; } = 2;
        /// <summary>死亡掉包模式: 0=不掉包, 1=全部掉落, 2=只掉腰带, 3=只掉背包</summary>
        public int DropOnDeath { get; set; }
        /// <summary>在线领地石硬度加成（0为无敌）</summary>
        public int LandClaimOnline { get; set; } = 4;
        /// <summary>离线领地石硬度加成（0为无敌）</summary>
        public int LandClaimOffline { get; set; } = 8;
        /// <summary>区域提示Buff名称</summary>
        public string? BuffName { get; set; }
        /// <summary>区域备注名称（可选）</summary>
        public string? Name { get; set; }
    }

    /// <summary>
    /// PVP/PVE 混合区域结果
    /// </summary>
    public class PvpVeAreaResult
    {
        /// <summary>唯一Id</summary>
        public int Id { get; set; }
        /// <summary>区域最小X坐标</summary>
        public int MinX { get; set; }
        /// <summary>区域最小Z坐标</summary>
        public int MinZ { get; set; }
        /// <summary>区域最大X坐标</summary>
        public int MaxX { get; set; }
        /// <summary>区域最大Z坐标</summary>
        public int MaxZ { get; set; }
        /// <summary>杀戮模式</summary>
        public int KillMode { get; set; }
        /// <summary>死亡掉包模式</summary>
        public int DropOnDeath { get; set; }
        /// <summary>在线领地石硬度加成</summary>
        public int LandClaimOnline { get; set; }
        /// <summary>离线领地石硬度加成</summary>
        public int LandClaimOffline { get; set; }
        /// <summary>区域提示Buff名称</summary>
        public string BuffName { get; set; } = string.Empty;
        /// <summary>区域备注名称</summary>
        public string? Name { get; set; }
        /// <summary>创建日期</summary>
        public DateTime CreatedAt { get; set; }
    }
}
