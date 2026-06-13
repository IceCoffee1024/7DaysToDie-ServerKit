using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 自定义商人保护区域仓储接口
    /// </summary>
    public interface ITraderProtectAreaRepository : IRepository<T_TraderProtectArea>
    {
        /// <summary>
        /// 获取所有商人保护区域
        /// </summary>
        Task<IEnumerable<T_TraderProtectArea>> GetAllAsync();

        /// <summary>
        /// 根据Id删除
        /// </summary>
        Task<int> DeleteByIdAsync(int id);

        /// <summary>
        /// 清空所有商人保护区域
        /// </summary>
        Task<int> DeleteAllAsync();
    }
}
