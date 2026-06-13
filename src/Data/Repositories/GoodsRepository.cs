using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 商品仓储
    /// </summary>
    public class GoodsRepository : DefaultRepository<T_Goods>, IGoodsRepository
    {
        /// <inheritdoc/>
        public Task<IEnumerable<T_Goods>> GetAllOrderByIdAsync()
        {
            return base.GetListAsync(orderByClause: "Id ASC");
        }
    }
}