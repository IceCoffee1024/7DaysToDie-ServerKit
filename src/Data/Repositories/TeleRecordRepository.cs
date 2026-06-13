using SdtdServerKit.Data;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 传送记录仓储
    /// </summary>
    public class TeleRecordRepository : DefaultRepository<T_TeleRecord>, ITeleRecordRepository
    {
        /// <inheritdoc/>
        public Task<T_TeleRecord?> GetNewestAsync(string playerId, TeleTargetType teleTargetType)
        {
            return base.GetFirstOrDefaultAsync(
                "PlayerId=@PlayerId AND TargetType=@TargetType", 
                "CreatedAt DESC LIMIT 1", 
                param: new { PlayerId = playerId, TargetType = teleTargetType.ToString() });
        }
    }
}