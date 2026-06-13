using System.ComponentModel.DataAnnotations;
using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// 公共回城点
    /// </summary>
    [Authorize]
    [RoutePrefix("api/CityLocation")]
    public class CityLocationController : ApiController
    {
        private readonly ICityLocationRepository _repository;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="repository"></param>
        public CityLocationController(ICityLocationRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 通过Id获取记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:int}")]
        [ResponseType(typeof(T_CityLocation))]
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
        /// 获取所有记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IEnumerable<T_CityLocation>> Get()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// 新增记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody] CityLocation model)
        {
            var entity = new T_CityLocation()
            {
                Id = model.Id,
                CityName = model.CityName,
                CreatedAt = DateTime.Now,
                PointsRequired = model.PointsRequired,
                Position = model.Position,
                ViewDirection = model.ViewDirection,
                IsEnabled = model.IsEnabled,
                TeleCommand = model.TeleCommand
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
        public async Task<IHttpActionResult> Put(int id, [FromBody] CityLocation model)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.CityName = model.CityName;
            entity.PointsRequired = model.PointsRequired;
            entity.Position = model.Position;
            entity.ViewDirection = model.ViewDirection;
            entity.IsEnabled = model.IsEnabled;
            entity.TeleCommand = model.TeleCommand;
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
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> Delete([FromUri, MinLength(1)] int[] ids)
        {
            int count = await _repository.DeleteByIdsAsync(ids, true);
            return Ok(count);
        }
    }
}