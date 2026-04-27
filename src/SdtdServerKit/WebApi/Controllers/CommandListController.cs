using IceCoffee.SimpleCRUD.Dtos;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// 命令列表
    /// </summary>
    [Authorize]
    [RoutePrefix("api/CommandList")]
    public partial class CommandListController : ApiController
    {
        private readonly ICommandListRepository _repository;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="repository"></param>
        public CommandListController(ICommandListRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 通过Id获取记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:int}")]
        [ResponseType(typeof(T_CommandList))]
        public async Task<IHttpActionResult> Get(string id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        /// <summary>
        /// 分页获取记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<PagedDto<T_CommandList>> Get([FromUri] PaginationQuery model)
        {
            var dto = new PaginationQueryDto()
            {
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                Keyword = model.Keyword,
            };
            return await _repository.GetPagedListAsync(dto);
        }

        /// <summary>
        /// 新增记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody] CommandList model)
        {
            var entity = new T_CommandList()
            {
                CreatedAt = DateTime.Now,
                Command = model.Command,
                InMainThread = model.InMainThread,
                Description = model.Description,
            };

            int id = await _repository.InsertAsync<int>(entity);
            return Ok(id);
        }

        /// <summary>
        /// 通过Id更新记录
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Put(string id, [FromBody] CommandList model)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Command = model.Command;
            entity.InMainThread = model.InMainThread;
            entity.Description = model.Description;

            await _repository.UpdateAsync(entity);
            return Ok();
        }

        /// <summary>
        /// 通过Id删除记录
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Delete(string id)
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
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> Delete([FromUri] int[]? ids, [FromUri] bool deleteAll = false)
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