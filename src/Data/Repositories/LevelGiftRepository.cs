using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 等级礼包仓储
    /// </summary>
    public class LevelGiftRepository : DefaultRepository<T_LevelGift>, ILevelGiftRepository
    {
        public Task<int> ResetClaimStateAsync()
        {
            string sql = $"UPDATE {SqlGenerator.TableName} SET ClaimState=0";
            return base.ExecuteAsync(sql, useTransaction: true);
        }
    }
}
