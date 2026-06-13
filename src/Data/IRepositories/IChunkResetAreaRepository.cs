using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 定时重置区域仓储接口
    /// </summary>
    public interface IChunkResetAreaRepository : IRepository<T_ChunkResetArea>
    {
        /// <summary>
        /// 获取所有重置区域
        /// </summary>
        Task<IEnumerable<T_ChunkResetArea>> GetAllAsync();

        /// <summary>
        /// 根据Id删除
        /// </summary>
        Task<int> DeleteByIdAsync(int id);

        /// <summary>
        /// 清空所有重置区域
        /// </summary>
        Task<int> DeleteAllAsync();
    }
}
