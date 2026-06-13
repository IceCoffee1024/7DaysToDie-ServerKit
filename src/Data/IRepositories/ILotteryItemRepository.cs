using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 抽奖物品关联仓储
    /// </summary>
    public interface ILotteryItemRepository : IRepository<T_LotteryItem>
    {
        /// <summary>
        /// 根据抽奖Id删除关联物品
        /// </summary>
        Task<int> DeleteByLotteryIdAsync(int lotteryId);

        /// <summary>
        /// 根据抽奖Id获取关联物品列表
        /// </summary>
        Task<IEnumerable<T_LotteryItem>> GetListByLotteryIdAsync(int lotteryId);

        /// <summary>
        /// 根据抽奖Id获取物品详情（带权重），一条SQL JOIN查询
        /// </summary>
        Task<IEnumerable<LotteryItemWithWeight>> GetItemsWithWeightByLotteryIdAsync(int lotteryId);
    }
}
