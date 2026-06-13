using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// Represents a Cd Key Redeem Record repository.
    /// </summary>
    public class CdKeyRedeemRecordRepository : DefaultRepository<CdKeyRedeemRecord>, ICdKeyRedeemRecordRepository
    {
        public Task<CdKeyRedeemRecord?> GetByKeyAndPlayerIdAsync(string key, string playerId)
        {
            return base.GetFirstOrDefaultAsync("[Key]=@Key AND [PlayerId]=@PlayerId", param: new { Key = key, PlayerId = playerId });
        }
    }
}