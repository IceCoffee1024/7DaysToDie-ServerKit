using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 区域命令禁用仓储接口
    /// </summary>
    public interface IMuteCommandAreaRepository : IRepository<T_MuteCommandArea>
    {
        /// <summary>
        /// 获取所有区域命令禁用记录
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<T_MuteCommandArea>> GetAllAsync();

        /// <summary>
        /// 删除指定区域的指定命令
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="minZ"></param>
        /// <param name="maxX"></param>
        /// <param name="maxZ"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        Task<int> DeleteByAreaAndCommandAsync(int minX, int minZ, int maxX, int maxZ, string command);

        /// <summary>
        /// 删除指定区域的所有命令
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="minZ"></param>
        /// <param name="maxX"></param>
        /// <param name="maxZ"></param>
        /// <returns></returns>
        Task<int> DeleteByAreaAsync(int minX, int minZ, int maxX, int maxZ);

        /// <summary>
        /// 清空所有区域命令禁用记录
        /// </summary>
        /// <returns></returns>
        Task<int> DeleteAllAsync();
    }
}
