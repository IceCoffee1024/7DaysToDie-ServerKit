using IceCoffee.SimpleCRUD;
using IceCoffee.SimpleCRUD.Dtos;
using SdtdServerKit.Data.Dtos;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 积分日志仓储接口
    /// </summary>
    public interface IPointLogRepository : IRepository<T_PointLog>
    {
        /// <summary>分页查询</summary>
        Task<PagedDto<T_PointLog>> GetPagedListAsync(PointLogQueryDto dto);

        /// <summary>获取总条数</summary>
        Task<int> CountAllAsync();

        /// <summary>清空所有日志</summary>
        Task<int> DeleteAllAsync();

        /// <summary>删除早于指定日期的日志</summary>
        Task<int> DeleteOlderThanAsync(DateTime threshold);
    }
}
