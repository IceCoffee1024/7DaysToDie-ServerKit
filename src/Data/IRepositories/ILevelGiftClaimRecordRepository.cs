using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 等级礼包领取记录仓储接口
    /// </summary>
    public interface ILevelGiftClaimRecordRepository : IRepository<T_LevelGiftClaimRecord>
    {
        /// <summary>
        /// 检查玩家是否已领取指定礼包
        /// </summary>
        Task<bool> HasClaimedAsync(string playerId, string giftId);

        /// <summary>
        /// 获取玩家已领取的礼包ID列表
        /// </summary>
        Task<List<string>> GetClaimedGiftIdsAsync(string playerId);
    }
}
