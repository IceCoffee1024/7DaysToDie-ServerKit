using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public class VipGiftItemRepository : DefaultRepository<T_VipGiftItem>, IVipGiftItemRepository
    {
        /// <inheritdoc/>
        public Task<int> DeleteByVipGiftIdAsync(string vipGiftId)
        {
            return base.DeleteAsync("VipGiftId=@VipGiftId", param: new { VipGiftId = vipGiftId });
        }
    }
}