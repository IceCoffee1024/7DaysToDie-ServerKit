using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// Represents a Cd Key Item repository.
    /// </summary>
    public class CdKeyItemRepository : DefaultRepository<CdKeyItem>, ICdKeyItemRepository
    {
        public Task<int> DeleteByCdKeyIdAsync(int cdKeyId)
        {
            return base.DeleteAsync("CdKeyId=@CdKeyId", param: new { CdKeyId = cdKeyId });
        }
    }
}