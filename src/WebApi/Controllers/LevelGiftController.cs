using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System.ComponentModel.DataAnnotations;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// 等级礼包
    /// </summary>
    [Authorize]
    [RoutePrefix("api/LevelGift")]
    public partial class LevelGiftController : ApiController
    {
        private readonly ILevelGiftRepository _levelGiftRepository;
        private readonly IItemListRepository _itemListRepository;
        private readonly ICommandListRepository _commandListRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="levelGiftRepository">等级礼包仓储</param>
        /// <param name="itemListRepository">物品列表仓储</param>
        /// <param name="commandListRepository">命令列表仓储</param>
        public LevelGiftController(ILevelGiftRepository levelGiftRepository, IItemListRepository itemListRepository, ICommandListRepository commandListRepository)
        {
            _levelGiftRepository = levelGiftRepository;
            _itemListRepository = itemListRepository;
            _commandListRepository = commandListRepository;
        }

        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        /// <param name="id">记录ID</param>
        /// <returns>HTTP操作结果</returns>
        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(T_LevelGift))]
        public async Task<IHttpActionResult> Get(string id)
        {
            var entity = await _levelGiftRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        /// <summary>
        /// 获取所有记录
        /// </summary>
        /// <returns>等级礼包列表</returns>
        [HttpGet]
        [Route("")]
        public async Task<IEnumerable<T_LevelGift>> Get()
        {
            return await _levelGiftRepository.GetAllAsync();
        }

        /// <summary>
        /// 添加新记录
        /// </summary>
        /// <param name="model">等级礼包模型</param>
        /// <returns>HTTP操作结果</returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody] LevelGift model)
        {
            // 对于通用礼包，生成唯一ID
            if (model.GiftType == 1)
            {
                model.Id = Guid.NewGuid().ToString();
            }
            else
            {
                // 对于玩家礼包，检查是否已存在
                var existing = await _levelGiftRepository.GetByIdAsync(model.Id);
                if (existing != null)
                {
                    return BadRequest($"玩家 {model.Id} 已存在等级礼包记录，请勿重复添加");
                }
            }

            var entity = new T_LevelGift()
            {
                Id = model.Id,
                Name = model.Name,
                PlayerName = model.PlayerName,
                RequiredLevel = model.RequiredLevel,
                CreatedAt = DateTime.Now,
                ClaimState = model.ClaimState,
                TotalClaimCount = model.TotalClaimCount,
                Description = model.Description,
                GiftType = model.GiftType,
            };
            await _levelGiftRepository.InsertAsync(entity);
            return Ok();
        }

        /// <summary>
        /// 根据ID更新记录
        /// </summary>
        /// <param name="id">记录ID</param>
        /// <param name="model">等级礼包模型</param>
        /// <returns>HTTP操作结果</returns>
        [HttpPut]
        [Route("{id}")]
        public async Task<IHttpActionResult> Put(string id, [FromBody] LevelGift model)
        {
            var entity = await _levelGiftRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = model.Name;
            entity.PlayerName = model.PlayerName;
            entity.RequiredLevel = model.RequiredLevel;
            entity.ClaimState = model.ClaimState;
            entity.TotalClaimCount = model.TotalClaimCount;
            entity.Description = model.Description;
            entity.GiftType = model.GiftType;

            await _levelGiftRepository.UpdateAsync(entity);
            return Ok();
        }

        /// <summary>
        /// 根据ID删除记录
        /// </summary>
        /// <param name="id">记录ID</param>
        /// <returns>HTTP操作结果</returns>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> Delete(string id)
        {
            int count = await _levelGiftRepository.DeleteByIdAsync(id);
            if (count == 0)
            {
                return NotFound();
            }

            return Ok();
        }

        /// <summary>
        /// 批量删除记录
        /// </summary>
        /// <param name="ids">记录ID数组</param>
        /// <param name="deleteAll">删除所有记录标志</param>
        /// <param name="resetAll">重置所有记录标志</param>
        /// <returns>HTTP操作结果</returns>
        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> Delete([FromUri] string[]? ids, [FromUri] bool deleteAll = false, [FromUri] bool resetAll = false)
        {
            int count = 0;

            if (deleteAll)
            {
                count = await _levelGiftRepository.DeleteAllAsync(true);
            }
            else if (resetAll)
            {
                count = await _levelGiftRepository.ResetClaimStateAsync();
            }
            else if (ids != null && ids.Length > 0)
            {
                count = await _levelGiftRepository.DeleteByIdsAsync(ids, true);
            }

            return Ok(count);
        }

        /// <summary>
        /// 获取礼包关联的物品列表
        /// </summary>
        /// <param name="id">礼包ID</param>
        /// <returns>物品列表</returns>
        [HttpGet]
        [Route("{id}/Items")]
        public async Task<IEnumerable<T_ItemList>> GetItems(string id)
        {
            var data = await _itemListRepository.GetListByLevelGiftIdAsync(id);
            return data;
        }

        /// <summary>
        /// 更新礼包关联的物品
        /// </summary>
        /// <param name="id">礼包ID</param>
        /// <param name="itemIds">物品ID数组</param>
        /// <returns>HTTP操作结果</returns>
        [HttpPut]
        [Route("{id}/Items")]
        public async Task<IHttpActionResult> PutItems(string id, [FromBody, Required] int[] itemIds)
        {
            var entity = await _levelGiftRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var entities = new List<T_LevelGiftItem>();
            foreach (var item in itemIds)
            {
                entities.Add(new T_LevelGiftItem()
                {
                    LevelGiftId = id,
                    ItemId = item
                });
            }

            using var unitOfWork = ModApi.ServiceContainer.Resolve<IUnitOfWorkFactory>().Create();
            var levelGiftItemRepository = unitOfWork.GetRepository<ILevelGiftItemRepository>();
            await levelGiftItemRepository.DeleteByLevelGiftIdAsync(id);
            await levelGiftItemRepository.InsertAsync(entities);
            unitOfWork.Commit();

            return Ok();
        }

        /// <summary>
        /// 获取礼包关联的命令列表
        /// </summary>
        /// <param name="id">礼包ID</param>
        /// <returns>命令列表</returns>
        [HttpGet]
        [Route("{id}/Commands")]
        public async Task<IEnumerable<T_CommandList>> GetCommands(string id)
        {
            var data = await _commandListRepository.GetListByLevelGiftIdAsync(id);
            return data;
        }

        /// <summary>
        /// 更新礼包关联的命令
        /// </summary>
        /// <param name="id">礼包ID</param>
        /// <param name="itemIds">命令ID数组</param>
        /// <returns>HTTP操作结果</returns>
        [HttpPut]
        [Route("{id}/Commands")]
        public async Task<IHttpActionResult> PutCommands(string id, [FromBody, Required] int[] itemIds)
        {
            var entity = await _levelGiftRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var entities = new List<T_LevelGiftCommand>();
            foreach (var item in itemIds)
            {
                entities.Add(new T_LevelGiftCommand()
                {
                    LevelGiftId = id,
                    CommandId = item
                });
            }

            using var unitOfWork = ModApi.ServiceContainer.Resolve<IUnitOfWorkFactory>().Create();
            var levelGiftCommandRepository = unitOfWork.GetRepository<ILevelGiftCommandRepository>();
            await levelGiftCommandRepository.DeleteByLevelGiftIdAsync(id);
            await levelGiftCommandRepository.InsertAsync(entities);
            unitOfWork.Commit();

            return Ok();
        }
    }
}
