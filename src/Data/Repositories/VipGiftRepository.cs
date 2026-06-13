using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 商品仓储
    /// </summary>
    public class VipGiftRepository : DefaultRepository<T_VipGift>, IVipGiftRepository
    {
        /// <inheritdoc/>
        public Task<int> ResetClaimStateAsync()
        {
            string sql = $"UPDATE {SqlGenerator.TableName} SET ClaimState=0";
            return base.ExecuteAsync(sql, useTransaction: true);
        }
    }
}