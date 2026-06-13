using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 传送记录仓储
    /// </summary>
    public interface ITeleRecordRepository : IRepository<T_TeleRecord>
    {
        /// <summary>
        /// 根据玩家Id和目标类型获取最新的传送记录
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="teleTargetType"></param>
        /// <returns></returns>
        Task<T_TeleRecord?> GetNewestAsync(string playerId, TeleTargetType teleTargetType);
    }
}