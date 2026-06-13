using IceCoffee.SimpleCRUD;
using IceCoffee.SimpleCRUD.Dtos;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// Item list repository
    /// </summary>
    public interface IItemListRepository : IRepository<T_ItemList>
    {
        /// <summary>
        /// 使用显式ID插入
        /// </summary>
        Task InsertWithIdAsync(T_ItemList entity);

        /// <summary>
        /// Get item list by CdKey Id
        /// </summary>
        /// <param name="cdKeyId">CdKey Id</param>
        /// <returns>List of items</returns>
        Task<IEnumerable<T_ItemList>> GetListByCdKeyIdAsync(int cdKeyId);

        /// <summary>
        /// Get item list by goods Id
        /// </summary>
        /// <param name="goodsId">Goods Id</param>
        /// <returns>List of items</returns>
        Task<IEnumerable<T_ItemList>> GetListByGoodsIdAsync(int goodsId);

        /// <summary>
        /// Get item list by VIP gift Id
        /// </summary>
        /// <param name="vipGiftId">VIP gift Id</param>
        /// <returns>List of items</returns>
        Task<IEnumerable<T_ItemList>> GetListByVipGiftIdAsync(string vipGiftId);

        /// <summary>
        /// Get paged item list
        /// </summary>
        /// <param name="dto">Pagination query DTO</param>
        /// <returns>Paged list of items</returns>
        Task<PagedDto<T_ItemList>> GetPagedListAsync(PaginationQueryDto dto);

        /// <summary>
        /// 获取物品列表礼包ID
        /// </summary>
        Task<IEnumerable<T_ItemList>> GetListByLevelGiftIdAsync(string levelGiftId);
    }
}