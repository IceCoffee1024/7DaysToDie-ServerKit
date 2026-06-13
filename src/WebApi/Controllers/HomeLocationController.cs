using IceCoffee.SimpleCRUD.Dtos;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System.ComponentModel.DataAnnotations;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// 私人回城点
    /// </summary>
    [Authorize]
    [RoutePrefix("api/HomeLocation")]
    public class HomeLocationController : ApiController
    {
        private readonly IHomeLocationRepository _repository;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="repository"></param>
        public HomeLocationController(IHomeLocationRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 通过Id获取记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:int}")]
        [ResponseType(typeof(T_HomeLocation))]
        public async Task<IHttpActionResult> Get(int id)
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
        public async Task<PagedDto<T_HomeLocation>> Get([FromUri] PaginationQuery model)
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
        public async Task<IHttpActionResult> Post([FromBody] HomeLocation model)
        {
            var entity = new T_HomeLocation()
            {
                HomeName = model.HomeName,
                CreatedAt = DateTime.Now,
                PlayerId = model.PlayerId,
                PlayerName = model.PlayerName,
                Position = model.Position,
            };
            await _repository.InsertAsync(entity);
            return Ok();
        }


        /// <summary>
        /// 通过Id更新记录
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Put(int id, [FromBody] HomeLocation model)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.HomeName = model.HomeName;
            entity.PlayerId = model.PlayerId;
            entity.PlayerName = model.PlayerName;
            entity.Position = model.Position;
            await _repository.UpdateAsync(entity);
            return Ok();
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