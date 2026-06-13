using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 等级礼包领取记录仓储
    /// </summary>
    public class LevelGiftClaimRecordRepository : DefaultRepository<T_LevelGiftClaimRecord>, ILevelGiftClaimRecordRepository
    {
        /// <summary>
        /// 检查玩家是否已领取指定礼包
        /// </summary>
        public async Task<bool> HasClaimedAsync(string playerId, string giftId)
        {
            string sql = $"SELECT COUNT(*) FROM {SqlGenerator.TableName} WHERE PlayerId=@PlayerId AND GiftId=@GiftId";
            var count = await base.ExecuteScalarAsync<int>(sql, new { PlayerId = playerId, GiftId = giftId });
            return count > 0;
        }

        /// <summary>
        /// 获取玩家已领取的礼包ID列表
        /// </summary>
        public async Task<List<string>> GetClaimedGiftIdsAsync(string playerId)
        {
            string sql = $"SELECT GiftId FROM {SqlGenerator.TableName} WHERE PlayerId=@PlayerId";
            var result = await base.ExecuteQueryAsync<string>(sql, new { PlayerId = playerId });
            return result.ToList();
        }
    }
}
