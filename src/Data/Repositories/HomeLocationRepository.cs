using IceCoffee.SimpleCRUD.Dtos;
using SdtdServerKit.Data;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System.Text;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 家园位置仓储
    /// </summary>
    public class HomeLocationRepository : DefaultRepository<T_HomeLocation>, IHomeLocationRepository
    {
        /// <inheritdoc/>
        public Task<int> DeleteByPlayerIdAndHomeNameAsync(string playerId, string homeName)
        {
            return base.DeleteAsync("PlayerId=@PlayerId AND HomeName=@HomeName", param: new { PlayerId = playerId, HomeName = homeName });
        }
        /// <inheritdoc/>
        public Task<T_HomeLocation?> GetByPlayerIdAndHomeNameAsync(string playerId, string homeName)
        {
            return base.GetFirstOrDefaultAsync("PlayerId=@PlayerId AND HomeName=@HomeName", param: new { PlayerId = playerId, HomeName = homeName });
        }
        /// <inheritdoc/>
        public Task<IEnumerable<T_HomeLocation>> GetByPlayerIdAsync(string playerId)
        {
            return base.GetListAsync("PlayerId=@PlayerId", param: new { PlayerId = playerId });
        }
        /// <inheritdoc/>
        public Task<int> GetRecordCountByPlayerIdIdAsync(string playerId)
        {
            return base.GetRecordCountAsync("PlayerId=@PlayerId", param: new { PlayerId = playerId });
        }

        /// <inheritdoc/>
        public Task<PagedDto<T_HomeLocation>> GetPagedListAsync(PaginationQueryDto dto)
        {
            var whereClauseSB = new StringBuilder("1=1");

            if (string.IsNullOrEmpty(dto.Keyword) == false)
            {
                whereClauseSB.Append(" AND (PlayerId=@Keyword OR HomeName LIKE '%'||@Keyword||'%' OR PlayerName LIKE '%'||@Keyword||'%')");
            }

            //string orderByClause;
            //if (string.IsNullOrEmpty(dto.Order) == false && typeof(V_HomeLocation).GetProperty(dto.Order) != null)
            //{
            //    orderByClause = dto.Order + (dto.Desc ? "DESC" : "ASC");
            //}
            //else
            //{
            //    orderByClause = "CreatedAt " + (dto.Desc ? "DESC" : "ASC");
            //}

            var param = new { Keyword = dto.Keyword };
            return GetPagedListAsync(dto.PageNumber, dto.PageSize, whereClauseSB.ToString(), orderByClause: "HomeName ASC", param);
        }
    }
}