using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// PVP/PVE 混合区域仓储实现
    /// </summary>
    public class PvpVeAreaRepository : DefaultRepository<T_PvpVeArea>, IPvpVeAreaRepository
    {
        /// <inheritdoc/>
        public Task<IEnumerable<T_PvpVeArea>> GetAllAsync()
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
