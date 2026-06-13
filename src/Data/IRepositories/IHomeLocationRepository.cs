using IceCoffee.SimpleCRUD;
using IceCoffee.SimpleCRUD.Dtos;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 家园位置仓储
    /// </summary>
    public interface IHomeLocationRepository : IRepository<T_HomeLocation>
    {
        /// <summary>
        /// 根据玩家Id获取家园位置
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        Task<IEnumerable<T_HomeLocation>> GetByPlayerIdAsync(string playerId);

        /// <summary>
        /// 根据玩家Id和家园名称获取家园位置
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="homeName"></param>
        /// <returns></returns>
        Task<T_HomeLocation?> GetByPlayerIdAndHomeNameAsync(string playerId, string homeName);

        /// <summary>
        /// 根据玩家Id和家园名称删除家园位置
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="homeName"></param>
        /// <returns></returns>
        Task<int> DeleteByPlayerIdAndHomeNameAsync(string playerId, string homeName);

        /// <summary>
        /// 根据玩家Id获取家园位置数量
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        Task<int> GetRecordCountByPlayerIdIdAsync(string playerId);

        /// <summary>
        /// 分页获取记录
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<PagedDto<T_HomeLocation>> GetPagedListAsync(PaginationQueryDto dto);
    }
}