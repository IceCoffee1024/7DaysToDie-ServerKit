using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// Represents a Cd Key repository.
    /// </summary>
    public class CdKeyRepository : DefaultRepository<CdKey>, ICdKeyRepository
    {
        public Task<CdKey?> GetByKeyAsync(string key)
        {
            return base.GetFirstOrDefaultAsync("[Key]=@Key", param: new { Key = key });
        }

        public Task<int> UpdateRedeemCount(int id)
        {
            string sql = "UPDATE CdKey SET RedeemCount = RedeemCount + 1 WHERE Id=@Id AND (MaxRedeemCount <= 0 OR RedeemCount < MaxRedeemCount)";
            return base.ExecuteAsync(sql, param: new { Id = id }, true);
        }
    }
}