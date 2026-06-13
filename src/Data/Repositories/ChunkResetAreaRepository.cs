using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 定时重置区域仓储实现
    /// </summary>
    public class ChunkResetAreaRepository : DefaultRepository<T_ChunkResetArea>, IChunkResetAreaRepository
    {
        public Task<IEnumerable<T_ChunkResetArea>> GetAllAsync()
        {
            return base.GetListAsync();
        }

        public Task<int> DeleteByIdAsync(int id)
        {
            return base.DeleteAsync("Id=@Id", param: new { Id = id });
        }

        public Task<int> DeleteAllAsync()
        {
            return base.DeleteAsync("1=1");
        }
    }
}
