using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 抽奖记录仓储
    /// </summary>
    public interface ILotteryRecordRepository : IRepository<T_LotteryRecord>
    {
        /// <summary>
        /// 根据玩家Id和抽奖Id获取记录
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="lotteryId"></param>
        /// <returns></returns>
        Task<T_LotteryRecord?> GetByPlayerIdAndLotteryIdAsync(string playerId, int lotteryId);

        /// <summary>
        /// 根据玩家Id和抽奖Id删除记录
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="lotteryId"></param>
        /// <returns></returns>
        Task<int> DeleteByPlayerIdAndLotteryIdAsync(string playerId, int lotteryId);
    }
}
