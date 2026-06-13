using SdtdServerKit.Models;
using SdtdServerKit.MuteCommandAreas;
using System.Collections.Generic;
using System.Linq;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// 区域命令禁用管理
    /// </summary>
    [Authorize]
    [RoutePrefix("api/MuteCommandArea")]
    public class MuteCommandAreaController : ApiController
    {
        /// <summary>
        /// 获取所有区域命令禁用列表
        /// </summary>
        [HttpGet]
        [Route("")]
        [ResponseType(typeof(IEnumerable<MuteCommandAreaInfo>))]
        public IHttpActionResult GetMuteCommands()
        {
            var areas = MuteCommandManager.GetAll();
            var result = areas.Select(a => new MuteCommandAreaInfo()
            {
                MinX = a.PosA.x,
                MinZ = a.PosA.y,
                MaxX = a.PosB.x,
                MaxZ = a.PosB.y,
                MutedCommands = a.MutedCommands,
            });
            return Ok(result);
        }

        /// <summary>
        /// 添加区域命令禁用
        /// </summary>
        [HttpPost]
        [Route("")]
        public IHttpActionResult AddMuteCommand([FromBody] MuteCommandRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.Command))
                return BadRequest("命令不能为空");
            MuteCommandManager.MuteCommand(model.X1, model.Z1, model.X2, model.Z2, model.Command);
            return Ok(new { message = "区域命令禁用已添加" });
        }

        /// <summary>
        /// 更新区域命令禁用
        /// </summary>
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateMuteCommand([FromBody] UpdateMuteCommandRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.NewCommand))
                return BadRequest("命令不能为空");

            bool success = MuteCommandManager.UpdateArea(
                model.OldX1, model.OldZ1, model.OldX2, model.OldZ2,
                model.NewX1, model.NewZ1, model.NewX2, model.NewZ2,
                model.NewCommand);

            if (success)
                return Ok(new { message = "区域命令禁用已更新" });
            else
                return NotFound();
        }

        /// <summary>
        /// 取消区域命令禁用
        /// </summary>
        [HttpDelete]
        [Route("")]
        public IHttpActionResult RemoveMuteCommand([FromUri] int x1, [FromUri] int z1,
            [FromUri] int x2, [FromUri] int z2, [FromUri] string command)
        {
            if (MuteCommandManager.UnMuteCommand(x1, z1, x2, z2, command))
                return Ok(new { message = "区域命令禁用已移除" });
            return NotFound();
        }

        /// <summary>
        /// 清空所有区域命令禁用
        /// </summary>
        [HttpDelete]
        [Route("All")]
        public IHttpActionResult ClearMuteCommands()
        {
            MuteCommandManager.Clear();
            return Ok(new { message = "所有区域命令禁用已清空" });
        }
    }
}
