using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 抽奖仓储
    /// </summary>
    public class LotteryRepository : DefaultRepository<T_Lottery>, ILotteryRepository
    {
        public Task<IEnumerable<T_Lottery>> GetAllOrderByIdAsync()
        {
            return base.GetListAsync(orderByClause: "Id ASC");
        }
    }
}
