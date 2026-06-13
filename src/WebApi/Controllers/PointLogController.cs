using IceCoffee.SimpleCRUD.Dtos;
using SdtdServerKit.Data.Dtos;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.Functions;
using SdtdServerKit.Managers;
using SdtdServerKit.Models;
using System.Threading.Tasks;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// 积分日志
    /// </summary>
    [Authorize]
    [RoutePrefix("api/PointLog")]
    public class PointLogController : ApiController
    {
        private IPointLogRepository GetRepo()
        {
            if (FunctionManager.TryGetFunction<PointLog>(out var function) && function != null)
            {
                return function.Repository;
            }
            return ModApi.ServiceContainer.Resolve<IPointLogRepository>();
        }

        /// <summary>
        /// 分页查询积分日志
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<PagedDto<T_PointLog>> Get([FromUri] PointLogQuery model)
        {
            var dto = new PointLogQueryDto
            {
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                Keyword = model.Keyword,
                Order = model.Order,
                Desc = model.Desc,
                StartDateTime = model.StartDateTime,
                EndDateTime = model.EndDateTime,
                Category = model.Category,
                ChangeType = model.ChangeType,
            };
            return await GetRepo().GetPagedListAsync(dto);
        }

        /// <summary>
        /// 获取总数
        /// </summary>
        [HttpGet]
        [Route("Count")]
        public async Task<int> GetCount()
        {
            return await GetRepo().CountAllAsync();
        }

        /// <summary>
        /// 删除单条日志
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            int affected = await GetRepo().DeleteByIdAsync(id);
            if (affected <= 0) return NotFound();
            return Ok();
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> Delete([FromUri] int[]? ids, [FromUri] bool deleteAll = false)
        {
            int count = 0;
            var repo = GetRepo();
            if (deleteAll)
            {
                count = await repo.DeleteAllAsync();
            }
            else if (ids != null && ids.Length > 0)
            {
                count = await repo.DeleteByIdsAsync(ids, useTransaction: true);
            }
            return Ok(count);
        }
    }
}
