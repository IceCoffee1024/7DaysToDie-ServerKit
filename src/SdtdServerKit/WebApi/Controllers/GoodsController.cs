using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System.ComponentModel.DataAnnotations;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// 商品
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Goods")]
    public partial class GoodsController : ApiController
    {
        private readonly IGoodsRepository _goodsRepository;
        private readonly IItemListRepository _itemListRepository;
        private readonly ICommandListRepository _commandListRepository;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="goodsRepository"></param>
        /// <param name="itemListRepository"></param>
        /// <param name="commandListRepository"></param>
        public GoodsController(IGoodsRepository goodsRepository, IItemListRepository itemListRepository, ICommandListRepository commandListRepository)
        {
            _goodsRepository = goodsRepository;
            _itemListRepository = itemListRepository;
            _commandListRepository = commandListRepository;
        }

        /// <summary>
        /// 通过Id获取记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:int}")]
        [ResponseType(typeof(T_Goods))]
        public async Task<IHttpActionResult> Get(int id)
        {
            var entity = await _goodsRepository.GetByIdAsync(id);
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
        public async Task<IEnumerable<T_Goods>> Get()
        {
            return await _goodsRepository.GetAllOrderByIdAsync();
        }

        /// <summary>
        /// 新增记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody] Goods model)
        {
            var entity = new T_Goods()
            {
                Id = model.Id,
                Name = model.Name,
                CreatedAt = DateTime.Now,
                Price = model.Price,
                Description = model.Description,
            };
            await _goodsRepository.InsertAsync(entity);
            return Ok();
        }


        /// <summary>
        /// 通过Id更新记录
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Put(int id, [FromBody] Goods model)
        {
            var entity = await _goodsRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = model.Name;
            entity.Price = model.Price;
            entity.Description = model.Description;
           
            await _goodsRepository.UpdateAsync(entity);
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
            int count = await _goodsRepository.DeleteByIdAsync(id);
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
        public async Task<IHttpActionResult> Delete([FromUri] int[]? ids, [FromUri] bool deleteAll = false)
        {
            int count = 0;

            if (deleteAll)
            {
               count = await _goodsRepository.DeleteAllAsync(true);
            }
            else if(ids != null && ids.Length > 0)
            {
                count = await _goodsRepository.DeleteByIdsAsync(ids, true);
            }

            return Ok(count);
        }

        /// <summary>
        /// 获取商品关联的物品清单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:int}/Items")]
        public async Task<IEnumerable<T_ItemList>> GetItems(int id)
        {
            var data = await _itemListRepository.GetListByGoodsIdAsync(id);
            return data;
        }

        /// <summary>
        /// 修改商品关联的物品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemIds"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id:int}/Items")]
        public async Task<IHttpActionResult> PutItems(int id, [FromBody, Required] int[] itemIds)
        {
            var entity = await _goodsRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var entities = new List<T_GoodsItem>();
            foreach (var item in itemIds)
            {
                entities.Add(new T_GoodsItem()
                {
                    GoodsId = id,
                    ItemId = item
                });
            }

            using var unitOfWork = ModApi.ServiceContainer.Resolve<IUnitOfWorkFactory>().Create();
            var userTagRepository = unitOfWork.GetRepository<IGoodsItemRepository>();
            await userTagRepository.DeleteByGoodsIdAsync(id);
            await userTagRepository.InsertAsync(entities);
            unitOfWork.Commit();

            return Ok();
        }

        /// <summary>
        /// 获取商品关联的命令Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:int}/Commands")]
        public async Task<IEnumerable<T_CommandList>> GetCommands(int id)
        {
            var data = await _commandListRepository.GetListByGoodsIdAsync(id);
            return data;
        }

        /// <summary>
        /// 修改商品关联的命令
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemIds"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id:int}/Commands")]
        public async Task<IHttpActionResult> PutCommands(int id, [FromBody, Required] int[] itemIds)
        {
            var entity = await _goodsRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var entities = new List<T_GoodsCommand>();
            foreach (var item in itemIds)
            {
                entities.Add(new T_GoodsCommand()
                {
                    GoodsId = id,
                    CommandId = item
                });
            }

            using var unitOfWork = ModApi.ServiceContainer.Resolve<IUnitOfWorkFactory>().Create();
            var userTagRepository = unitOfWork.GetRepository<IGoodsCommandRepository>();
            await userTagRepository.DeleteByGoodsIdAsync(id);
            await userTagRepository.InsertAsync(entities);
            unitOfWork.Commit();

            return Ok();
        }
    }
}