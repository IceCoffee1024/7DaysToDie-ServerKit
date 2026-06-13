using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGoodsItemRepository : IRepository<T_GoodsItem>
    {
        /// <summary>
        /// 通过商品Id删除记录
        /// </summary>
        /// <param name="goodsId"></param>
        /// <returns></returns>
        Task<int> DeleteByGoodsIdAsync(int goodsId);
    }
}