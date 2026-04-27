using System.ComponentModel.DataAnnotations;
using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.Dtos;
using IceCoffee.SimpleCRUD.Dtos;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// 聊天记录
    /// </summary>
    [Authorize]
    [RoutePrefix("api/ChatRecord")]
    public class ChatRecordController : ApiController
    {
        private readonly IChatRecordRepository _repository;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="chatRecordRepository"></param>
        public ChatRecordController(IChatRecordRepository chatRecordRepository) 
        {
            _repository = chatRecordRepository;
        }

        /// <summary>
        /// 分页获取记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<PagedDto<T_ChatRecord>> Get([FromUri] ChatRecordQuery model)
        {
            var dto = new ChatRecordQueryDto()
            {
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                StartDateTime = model.StartDateTime,
                EndDateTime = model.EndDateTime,
                Keyword = model.Keyword,
                Order = model.Order,
                Desc = model.Desc,
                ChatType = model.ChatType,
                ExcludeSystemMessages = model.ExcludeSystemMessages
            };
            var data = await _repository.GetPagedListAsync(dto);
            return data;
        }

        /// <summary>
        /// 通过Id删除记录
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            int count = await _repository.DeleteByIdAsync(id);
            if (count == 0)
            {
                return NotFound();
            }

            return Ok();
        }

        /// <summary>
        /// 批量删除记录
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="deleteAll"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> Delete([FromUri] string[]? ids, [FromUri] bool deleteAll = false)
        {
            int count = 0;

            if (deleteAll)
            {
                count = await _repository.DeleteAllAsync(true);
            }
            else if (ids != null && ids.Length > 0)
            {
                count = await _repository.DeleteByIdsAsync(ids, true);
            }

            return Ok(count);
        }
    }
}