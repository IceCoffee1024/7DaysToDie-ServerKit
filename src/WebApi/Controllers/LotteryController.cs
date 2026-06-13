using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Dtos;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System.ComponentModel.DataAnnotations;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// 抽奖
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Lottery")]
    public partial class LotteryController : ApiController
    {
        private readonly ILotteryRepository _lotteryRepository;
        private readonly ILotteryItemRepository _lotteryItemRepository;
        private readonly ILotteryCommandRepository _lotteryCommandRepository;
        private readonly IItemListRepository _itemListRepository;
        private readonly ICommandListRepository _commandListRepository;

        /// <summary>
        /// 构造方法
        /// </summary>
        public LotteryController(
            ILotteryRepository lotteryRepository,
            ILotteryItemRepository lotteryItemRepository,
            ILotteryCommandRepository lotteryCommandRepository,
            IItemListRepository itemListRepository,
            ICommandListRepository commandListRepository)
        {
            _lotteryRepository = lotteryRepository;
            _lotteryItemRepository = lotteryItemRepository;
            _lotteryCommandRepository = lotteryCommandRepository;
            _itemListRepository = itemListRepository;
            _commandListRepository = commandListRepository;
        }

        /// <summary>
        /// 通过Id获取记录
        /// </summary>
        [HttpGet]
        [Route("{id:int}")]
        [ResponseType(typeof(T_Lottery))]
        public async Task<IHttpActionResult> Get(int id)
        {
            var entity = await _lotteryRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        /// <summary>
        /// 获取所有记录
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<IEnumerable<T_Lottery>> Get()
        {
            return await _lotteryRepository.GetAllOrderByIdAsync();
        }

        /// <summary>
        /// 新增记录
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody] LotteryDto model)
        {
            var entity = new T_Lottery()
            {
                Id = model.Id,
                Name = model.Name,
                IsEnabled = model.IsEnabled,
                DrawCommand = model.DrawCommand,
                DrawInterval = model.DrawInterval,
                DrawCost = model.DrawCost,
                Description = model.Description,
                CreatedAt = DateTime.Now,
            };
            await _lotteryRepository.InsertAsync(entity);
            return Ok();
        }

        /// <summary>
        /// 通过Id更新记录
        /// </summary>
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Put(int id, [FromBody] LotteryDto model)
        {
            var entity = await _lotteryRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = model.Name;
            entity.IsEnabled = model.IsEnabled;
            entity.DrawCommand = model.DrawCommand;
            entity.DrawInterval = model.DrawInterval;
            entity.DrawCost = model.DrawCost;
            entity.Description = model.Description;

            await _lotteryRepository.UpdateAsync(entity);
            return Ok();
        }

        /// <summary>
        /// 通过Id删除记录
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            int count = await _lotteryRepository.DeleteByIdAsync(id);
            if (count == 0)
            {
                return NotFound();
            }

            return Ok();
        }

        /// <summary>
        /// 批量删除记录
        /// </summary>
        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> Delete([FromUri] int[]? ids, [FromUri] bool deleteAll = false)
        {
            int count = 0;

            if (deleteAll)
            {
                count = await _lotteryRepository.DeleteAllAsync(true);
            }
            else if (ids != null && ids.Length > 0)
            {
                count = await _lotteryRepository.DeleteByIdsAsync(ids, true);
            }

            return Ok(count);
        }

        /// <summary>
        /// 获取抽奖关联的物品清单（带权重）
        /// </summary>
        [HttpGet]
        [Route("{id:int}/Items")]
        public async Task<IHttpActionResult> GetItems(int id)
        {
            var result = await _lotteryItemRepository.GetItemsWithWeightByLotteryIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// 修改抽奖关联的物品（带权重）
        /// </summary>
        [HttpPut]
        [Route("{id:int}/Items")]
        public async Task<IHttpActionResult> PutItems(int id, [FromBody, Required] LotteryItemWithWeightDto[] items)
        {
            var entity = await _lotteryRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var entities = new List<T_LotteryItem>();
            foreach (var item in items)
            {
                entities.Add(new T_LotteryItem()
                {
                    LotteryId = id,
                    ItemId = item.ItemId,
                    Weight = item.Weight
                });
            }

            using var unitOfWork = ModApi.ServiceContainer.Resolve<IUnitOfWorkFactory>().Create();
            var lotteryItemRepository = unitOfWork.GetRepository<ILotteryItemRepository>();
            await lotteryItemRepository.DeleteByLotteryIdAsync(id);
            if (entities.Count > 0)
            {
                await lotteryItemRepository.InsertAsync(entities);
            }
            unitOfWork.Commit();

            return Ok();
        }

        /// <summary>
        /// 获取抽奖关联的命令奖品（带权重）
        /// </summary>
        [HttpGet]
        [Route("{id:int}/Commands")]
        public async Task<IHttpActionResult> GetCommands(int id)
        {
            var result = await _lotteryCommandRepository.GetCommandsWithWeightByLotteryIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// 修改抽奖关联的命令奖品（带权重）
        /// </summary>
        [HttpPut]
        [Route("{id:int}/Commands")]
        public async Task<IHttpActionResult> PutCommands(int id, [FromBody, Required] LotteryCommandWithWeightDto[] commands)
        {
            var entity = await _lotteryRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var entities = new List<T_LotteryCommand>();
            foreach (var item in commands)
            {
                entities.Add(new T_LotteryCommand()
                {
                    LotteryId = id,
                    CommandId = item.CommandId,
                    Weight = item.Weight
                });
            }

            using var unitOfWork = ModApi.ServiceContainer.Resolve<IUnitOfWorkFactory>().Create();
            var lotteryCommandRepository = unitOfWork.GetRepository<ILotteryCommandRepository>();
            await lotteryCommandRepository.DeleteByLotteryIdAsync(id);
            if (entities.Count > 0)
            {
                await lotteryCommandRepository.InsertAsync(entities);
            }
            unitOfWork.Commit();

            return Ok();
        }
    }
}
