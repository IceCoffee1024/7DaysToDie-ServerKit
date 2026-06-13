using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public class GoodsItemRepository : DefaultRepository<T_GoodsItem>, IGoodsItemRepository
    {
        /// <inheritdoc/>
        public Task<int> DeleteByGoodsIdAsync(int goodsId)
        {
            return base.DeleteAsync("GoodsId=@GoodsId", param: new { GoodsId = goodsId });
        }
    }
}