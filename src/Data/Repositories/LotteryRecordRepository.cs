using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 抽奖记录仓储
    /// </summary>
    public class LotteryRecordRepository : DefaultRepository<T_LotteryRecord>, ILotteryRecordRepository
    {
        public async Task<T_LotteryRecord?> GetByPlayerIdAndLotteryIdAsync(string playerId, int lotteryId)
        {
            var list = await base.GetListAsync("PlayerId=@PlayerId AND LotteryId=@LotteryId", param: new { PlayerId = playerId, LotteryId = lotteryId });
            return list.FirstOrDefault();
        }

        public Task<int> DeleteByPlayerIdAndLotteryIdAsync(string playerId, int lotteryId)
        {
            return base.DeleteAsync("PlayerId=@PlayerId AND LotteryId=@LotteryId", param: new { PlayerId = playerId, LotteryId = lotteryId });
        }
    }
}
