using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 自定义商人保护区域仓储实现
    /// </summary>
    public class TraderProtectAreaRepository : DefaultRepository<T_TraderProtectArea>, ITraderProtectAreaRepository
    {
        /// <inheritdoc/>
        public Task<IEnumerable<T_TraderProtectArea>> GetAllAsync()
        {
            return base.GetListAsync();
        }

        /// <inheritdoc/>
        public Task<int> DeleteByIdAsync(int id)
        {
            return base.DeleteAsync("Id=@Id", param: new { Id = id });
        }

        /// <inheritdoc/>
        public Task<int> DeleteAllAsync()
        {
            return base.DeleteAsync("1=1");
        }
    }
}
