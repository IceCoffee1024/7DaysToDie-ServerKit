using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// PVP/PVE 混合区域仓储接口
    /// </summary>
    public interface IPvpVeAreaRepository : IRepository<T_PvpVeArea>
    {
        /// <summary>
        /// 获取所有 PVP/PVE 混合区域
        /// </summary>
        Task<IEnumerable<T_PvpVeArea>> GetAllAsync();

        /// <summary>
        /// 根据Id删除
        /// </summary>
        Task<int> DeleteByIdAsync(int id);

        /// <summary>
        /// 清空所有 PVP/PVE 混合区域
        /// </summary>
        Task<int> DeleteAllAsync();
    }
}
