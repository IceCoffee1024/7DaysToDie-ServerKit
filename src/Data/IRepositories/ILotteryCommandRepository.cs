using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 抽奖命令关联仓储接口
    /// </summary>
    public interface ILotteryCommandRepository : IRepository<T_LotteryCommand>
    {
        /// <summary>
        /// 根据抽奖Id删除关联命令
        /// </summary>
        Task<int> DeleteByLotteryIdAsync(int lotteryId);

        /// <summary>
        /// 根据抽奖Id获取关联命令列表
        /// </summary>
        Task<IEnumerable<T_LotteryCommand>> GetListByLotteryIdAsync(int lotteryId);

        /// <summary>
        /// 根据抽奖Id获取命令奖品详情（带权重），一条SQL JOIN查询
        /// </summary>
        Task<IEnumerable<LotteryCommandWithWeight>> GetCommandsWithWeightByLotteryIdAsync(int lotteryId);
    }
}
