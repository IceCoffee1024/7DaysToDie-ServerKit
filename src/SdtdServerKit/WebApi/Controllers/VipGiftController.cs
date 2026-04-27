using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System.ComponentModel.DataAnnotations;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// Gift
    /// </summary>
    [Authorize]
    [RoutePrefix("api/VipGift")]
    public partial class VipGiftController : ApiController
    {
        private readonly IVipGiftRepository _vipGiftRepository;
        private readonly IItemListRepository _itemListRepository;
        private readonly ICommandListRepository _commandListRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vipGiftRepository">VIP gift repository</param>
        /// <param name="itemListRepository">Item list repository</param>
        /// <param name="commandListRepository">Command list repository</param>
        public VipGiftController(IVipGiftRepository vipGiftRepository, IItemListRepository itemListRepository, ICommandListRepository commandListRepository)
        {
            _vipGiftRepository = vipGiftRepository;
            _itemListRepository = itemListRepository;
            _commandListRepository = commandListRepository;
        }

        /// <summary>
        /// Get record by Id
        /// </summary>
        /// <param name="id">Record Id</param>
        /// <returns>HTTP action result</returns>
        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(T_VipGift))]
        public async Task<IHttpActionResult> Get(string id)
        {
            var entity = await _vipGiftRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        /// <summary>
        /// Get all records
        /// </summary>
        /// <returns>List of VIP gifts</returns>
        [HttpGet]
        [Route("")]
        public async Task<IEnumerable<T_VipGift>> Get()
        {
            return await _vipGiftRepository.GetAllAsync();
        }

        /// <summary>
        /// Add new record
        /// </summary>
        /// <param name="model">VIP gift model</param>
        /// <returns>HTTP action result</returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody] VipGift model)
        {
            var existing = await _vipGiftRepository.GetByIdAsync(model.Id);
            if (existing != null)
            {
                return BadRequest($"玩家 {model.Id} 已存在VIP礼包记录，请勿重复添加");
            }

            var entity = new T_VipGift()
            {
                Id = model.Id,
                Name = model.Name,
                PlayerName = model.PlayerName,
                CreatedAt = DateTime.Now,
                ClaimState = model.ClaimState,
                TotalClaimCount = model.TotalClaimCount,
                Description = model.Description,
            };
            await _vipGiftRepository.InsertAsync(entity);
            return Ok();
        }

        /// <summary>
        /// Update record by Id
        /// </summary>
        /// <param name="id">Record Id</param>
        /// <param name="model">VIP gift model</param>
        /// <returns>HTTP action result</returns>
        [HttpPut]
        [Route("{id}")]
        public async Task<IHttpActionResult> Put(string id, [FromBody] VipGift model)
        {
            var entity = await _vipGiftRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = model.Name;
            entity.PlayerName = model.PlayerName;
            entity.ClaimState = model.ClaimState;
            entity.TotalClaimCount = model.TotalClaimCount;
            entity.Description = model.Description;

            await _vipGiftRepository.UpdateAsync(entity);
            return Ok();
        }

        /// <summary>
        /// Delete record by Id
        /// </summary>
        /// <param name="id">Record Id</param>
        /// <returns>HTTP action result</returns>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> Delete(string id)
        {
            int count = await _vipGiftRepository.DeleteByIdAsync(id);
            if (count == 0)
            {
                return NotFound();
            }

            return Ok();
        }

        /// <summary>
        /// Batch delete records
        /// </summary>
        /// <param name="ids">Array of record Ids</param>
        /// <param name="deleteAll">Flag to delete all records</param>
        /// <param name="resetAll">Flag to reset all records</param>
        /// <returns>HTTP action result</returns>
        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> Delete([FromUri] string[]? ids, [FromUri] bool deleteAll = false, [FromUri] bool resetAll = false)
        {
            int count = 0;

            if (deleteAll)
            {
                count = await _vipGiftRepository.DeleteAllAsync(true);
            }
            else if (resetAll)
            {
                count = await _vipGiftRepository.ResetClaimStateAsync();
            }
            else if (ids != null && ids.Length > 0)
            {
                count = await _vipGiftRepository.DeleteByIdsAsync(ids, true);
            }

            return Ok(count);
        }

        /// <summary>
        /// Get item list associated with the gift
        /// </summary>
        /// <param name="id">Gift Id</param>
        /// <returns>List of items</returns>
        [HttpGet]
        [Route("{id}/Items")]
        public async Task<IEnumerable<T_ItemList>> GetItems(string id)
        {
            var data = await _itemListRepository.GetListByVipGiftIdAsync(id);
            return data;
        }

        /// <summary>
        /// Update items associated with the gift
        /// </summary>
        /// <param name="id">Gift Id</param>
        /// <param name="itemIds">Array of item Ids</param>
        /// <returns>HTTP action result</returns>
        [HttpPut]
        [Route("{id}/Items")]
        public async Task<IHttpActionResult> PutItems(string id, [FromBody, Required] int[] itemIds)
        {
            var entity = await _vipGiftRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var entities = new List<T_VipGiftItem>();
            foreach (var item in itemIds)
            {
                entities.Add(new T_VipGiftItem()
                {
                    VipGiftId = id,
                    ItemId = item
                });
            }

            using var unitOfWork = ModApi.ServiceContainer.Resolve<IUnitOfWorkFactory>().Create();
            var vipGiftItemRepository = unitOfWork.GetRepository<IVipGiftItemRepository>();
            await vipGiftItemRepository.DeleteByVipGiftIdAsync(id);
            await vipGiftItemRepository.InsertAsync(entities);
            unitOfWork.Commit();

            return Ok();
        }

        /// <summary>
        /// Get command list associated with the gift
        /// </summary>
        /// <param name="id">Gift Id</param>
        /// <returns>List of commands</returns>
        [HttpGet]
        [Route("{id}/Commands")]
        public async Task<IEnumerable<T_CommandList>> GetCommands(string id)
        {
            var data = await _commandListRepository.GetListByVipGiftIdAsync(id);
            return data;
        }

        /// <summary>
        /// Update commands associated with the gift
        /// </summary>
        /// <param name="id">Gift Id</param>
        /// <param name="itemIds">Array of command Ids</param>
        /// <returns>HTTP action result</returns>
        [HttpPut]
        [Route("{id}/Commands")]
        public async Task<IHttpActionResult> PutCommands(string id, [FromBody, Required] int[] itemIds)
        {
            var entity = await _vipGiftRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var entities = new List<T_VipGiftCommand>();
            foreach (var item in itemIds)
            {
                entities.Add(new T_VipGiftCommand()
                {
                    VipGiftId = id,
                    CommandId = item
                });
            }

            using var unitOfWork = ModApi.ServiceContainer.Resolve<IUnitOfWorkFactory>().Create();
            var vipGiftCommandRepository = unitOfWork.GetRepository<IVipGiftCommandRepository>();
            await vipGiftCommandRepository.DeleteByVipGiftIdAsync(id);
            await vipGiftCommandRepository.InsertAsync(entities);
            unitOfWork.Commit();

            return Ok();
        }
    }
}