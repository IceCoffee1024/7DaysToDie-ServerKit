using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// Colored Chat
    /// </summary>
    [Authorize]
    [RoutePrefix("api/ColoredChat")]
    public class ColoredChatController : ApiController
    {
        private readonly IColoredChatRepository _coloredChatRepository;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="coloredChatRepository"></param>
        public ColoredChatController(IColoredChatRepository coloredChatRepository)
        {
            _coloredChatRepository = coloredChatRepository;
        }

        /// <summary>
        /// 通过Id获取记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(T_ColoredChat))]
        public async Task<IHttpActionResult> Get(string id)
        {
            var entity = await _coloredChatRepository.GetByIdAsync(id);
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
        public async Task<IEnumerable<T_ColoredChat>> Get()
        {
            return await _coloredChatRepository.GetAllAsync();
        }

        /// <summary>
        /// 新增记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody] ColoredChat model)
        {
            var entity = new T_ColoredChat()
            {
                Id = model.Id,
                CreatedAt = DateTime.Now,
                CustomName = model.CustomName,
                NameColor = model.NameColor,
                TextColor = model.TextColor,
                Description = model.Description,
            };
            await _coloredChatRepository.InsertAsync(entity);
            return Ok();
        }


        /// <summary>
        /// 通过Id更新记录
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}")]
        public async Task<IHttpActionResult> Put(string id, [FromBody] ColoredChat model)
        {
            var entity = await _coloredChatRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.CustomName = model.CustomName;
            entity.NameColor = model.NameColor;
            entity.TextColor = model.TextColor;
            entity.Description = model.Description;
           
            await _coloredChatRepository.UpdateAsync(entity);
            return Ok();
        }

        /// <summary>
        /// 通过Id删除记录
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> Delete(string id)
        {
            int count = await _coloredChatRepository.DeleteByIdAsync(id);
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
               count = await _coloredChatRepository.DeleteAllAsync(true);
            }
            else if (ids != null && ids.Length > 0)
            {
                count = await _coloredChatRepository.DeleteByIdsAsync(ids, true);
            }

            return Ok(count);
        }
    }
}