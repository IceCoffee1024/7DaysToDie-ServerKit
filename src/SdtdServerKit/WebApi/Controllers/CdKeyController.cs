using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.Dtos;
using SdtdServerKit.Data.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Mapster;
using IceCoffee.SimpleCRUD;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// Cd Key
    /// </summary>
    [Authorize]
    [RoutePrefix("api/CdKeys")]
    public partial class CdKeysController : ApiController
    {
        private readonly ICdKeyRepository _cdKeyRepository;
        private readonly IItemListRepository _itemListRepository;
        private readonly ICommandListRepository _commandListRepository;

        public CdKeysController(ICdKeyRepository cdKeyRepository, IItemListRepository itemListRepository, ICommandListRepository commandListRepository)
        {
            _cdKeyRepository = cdKeyRepository;
            _itemListRepository = itemListRepository;
            _commandListRepository = commandListRepository;
        }

        /// <summary>
        /// Get all Cd Keys.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IEnumerable<CdKeyDto>> GetCdKeys()
        {
            return (await _cdKeyRepository.GetAllAsync()).Adapt<IEnumerable<CdKeyDto>>();
        }

        /// <summary>
        /// Get a Cd Key by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(CdKeyDto))]
        public async Task<IHttpActionResult> GetCdKey([FromUri] int id)
        {
            var entity = await _cdKeyRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity.Adapt<CdKey>());
        }

        /// <summary>
        /// Creates a new Cd Key.
        /// </summary>
        /// <param name="dto">The Cd Key dto to create.</param>
        /// <returns>An IHttpActionResult indicating the result of the operation.</returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> CreateCdKey([FromBody] CdKeyCreateDto dto)
        {
            var entity = dto.Adapt<CdKey>();
            entity.CreatedAt = DateTime.Now;
            int id = await _cdKeyRepository.InsertAsync<int>(entity);
            var location = new Uri(Request.RequestUri, $"api/CdKey/{id}");

            return Created(location, entity);
        }

        /// <summary>
        /// Updates an existing Cd Key.
        /// </summary>
        /// <param name="id">The ID of the Cd Key to update.</param>
        /// <param name="dto">The updated Cd Key dto.</param>
        /// <returns>An IHttpActionResult indicating the result of the operation.</returns>
        [HttpPut]
        [Route("{id}")]
        public async Task<IHttpActionResult> UpdateCdKey([FromUri] int id, [FromBody] CdKeyUpdateDto dto)
        {
            var entity = await _cdKeyRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            dto.Adapt(entity);
            await _cdKeyRepository.UpdateAsync(entity);

            return Ok();
        }

        /// <summary>
        /// Deletes a Cd Key by ID.
        /// </summary>
        /// <param name="id">The ID of the Cd Key to delete.</param>
        /// <returns>An IHttpActionResult indicating the result of the operation.</returns>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> DeleteCdKey([FromUri] int id)
        {
            int count = await _cdKeyRepository.DeleteByIdAsync(id);
            if (count == 0)
            {
                return NotFound();
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes multiple Cd Key by their IDs.
        /// </summary>
        /// <param name="ids">The IDs of the Cd Key to delete.</param>
        /// <returns>An IHttpActionResult indicating the result of the operation.</returns>
        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> DeleteCdKeys([FromUri, Required, MinLength(1)] int[] ids)
        {
            int count = await _cdKeyRepository.DeleteByIdsAsync(ids, true);
            if (count == 0)
            {
                return NotFound();
            }
            return StatusCode(HttpStatusCode.NoContent);
        }


        /// <summary>
        /// Get item list associated with the Cd Key ID.
        /// </summary>
        /// <param name="id">Cd Key Id</param>
        /// <returns>List of items</returns>
        [HttpGet]
        [Route("{id}/Items")]
        public async Task<IEnumerable<T_ItemList>> GetItems(int id)
        {
            var data = await _itemListRepository.GetListByCdKeyIdAsync(id);
            return data;
        }

        /// <summary>
        /// Update items associated with the Cd Key ID.
        /// </summary>
        /// <param name="id">Cd Key Id</param>
        /// <param name="itemIds">Array of item Ids</param>
        /// <returns>HTTP action result</returns>
        [HttpPut]
        [Route("{id}/Items")]
        public async Task<IHttpActionResult> UpdateItems(int id, [FromBody, Required] int[] itemIds)
        {
            var entity = await _cdKeyRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var entities = new List<CdKeyItem>();
            foreach (var item in itemIds)
            {
                entities.Add(new CdKeyItem()
                {
                    CdKeyId = id,
                    ItemId = item
                });
            }

            using var unitOfWork = ModApi.ServiceContainer.Resolve<IUnitOfWorkFactory>().Create();
            var cdKeyItemRepository = unitOfWork.GetRepository<ICdKeyItemRepository>();
            await cdKeyItemRepository.DeleteByCdKeyIdAsync(id);
            await cdKeyItemRepository.InsertAsync(entities);
            unitOfWork.Commit();

            return Ok();
        }

        /// <summary>
        /// Get command list associated with the Cd Key ID.
        /// </summary>
        /// <param name="id">Cd Key Id</param>
        /// <returns>List of commands</returns>
        [HttpGet]
        [Route("{id}/Commands")]
        public async Task<IEnumerable<T_CommandList>> GetCommands(int id)
        {
            var data = await _commandListRepository.GetListByCdKeyIdAsync(id);
            return data;
        }

        /// <summary>
        /// Update commands associated with the Cd Key ID.
        /// </summary>
        /// <param name="id">Cd Key Id</param>
        /// <param name="itemIds">Array of command Ids</param>
        /// <returns>HTTP action result</returns>
        [HttpPut]
        [Route("{id}/Commands")]
        public async Task<IHttpActionResult> UpdateCommands(int id, [FromBody, Required] int[] itemIds)
        {
            var entity = await _cdKeyRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var entities = new List<CdKeyCommand>();
            foreach (var item in itemIds)
            {
                entities.Add(new CdKeyCommand()
                {
                    CdKeyId = id,
                    CommandId = item
                });
            }

            using var unitOfWork = ModApi.ServiceContainer.Resolve<IUnitOfWorkFactory>().Create();
            var cdKeyCommandRepository = unitOfWork.GetRepository<ICdKeyCommandRepository>();
            await cdKeyCommandRepository.DeleteByCdKeyIdAsync(id);
            await cdKeyCommandRepository.InsertAsync(entities);
            unitOfWork.Commit();

            return Ok();
        }
    }
}
