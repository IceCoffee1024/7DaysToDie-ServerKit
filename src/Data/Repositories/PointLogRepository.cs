using IceCoffee.SimpleCRUD.Dtos;
using SdtdServerKit.Data.Dtos;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System.Text;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 积分日志仓储实现
    /// </summary>
    public class PointLogRepository : DefaultRepository<T_PointLog>, IPointLogRepository
    {
        public Task<PagedDto<T_PointLog>> GetPagedListAsync(PointLogQueryDto dto)
        {
            var sb = new StringBuilder("1=1");

            if (dto.StartDateTime.HasValue)
            {
                sb.Append(" AND CreatedAt>=@StartDateTime");
            }
            if (dto.EndDateTime.HasValue)
            {
                sb.Append(" AND CreatedAt<=@EndDateTime");
            }
            if (dto.Category.HasValue)
            {
                sb.Append(" AND Category=@Category");
            }
            if (dto.ChangeType.HasValue)
            {
                sb.Append(" AND ChangeType=@ChangeType");
            }
            if (string.IsNullOrEmpty(dto.Keyword) == false)
            {
                sb.Append(" AND (PlayerId=@Keyword OR PlayerName LIKE '%'||@Keyword||'%' OR Note LIKE '%'||@Keyword||'%')");
            }

            string orderByClause = dto.Order + (dto.Desc ? " DESC" : " ASC");

            var param = new
            {
                Keyword = dto.Keyword,
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                Category = (int?)dto.Category,
                ChangeType = (int?)dto.ChangeType,
            };
            return base.GetPagedListAsync(dto.PageNumber, dto.PageSize, sb.ToString(), orderByClause, param);
        }

        public Task<int> CountAllAsync()
        {
            string sql = $"SELECT COUNT(*) FROM {SqlGenerator.TableName}";
            return base.ExecuteScalarAsync<int>(sql);
        }

        public Task<int> DeleteAllAsync()
        {
            return base.DeleteAsync("1=1");
        }

        public Task<int> DeleteOlderThanAsync(DateTime threshold)
        {
            return base.DeleteAsync("CreatedAt<@Threshold", new { Threshold = threshold });
        }
    }
}
