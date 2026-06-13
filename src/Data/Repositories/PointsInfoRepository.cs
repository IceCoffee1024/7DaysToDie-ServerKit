using IceCoffee.SimpleCRUD.Dtos;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System.Text;
using Webserver.WebAPI.APIs.WorldState;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// Represents a repository for managing points information.
    /// </summary>
    public class PointsInfoRepository : DefaultRepository<T_PointsInfo>, IPointsInfoRepository
    {
        /// <summary>
        /// Gets the points by player ID asynchronously.
        /// </summary>
        /// <param name="playerId">The player ID.</param>
        /// <returns>The points of the player.</returns>
        public Task<int> GetPointsByIdAsync(string playerId)
        {
            string sql = $"SELECT Points FROM {SqlGenerator.TableName} WHERE Id=@PlayerId";
            return base.ExecuteScalarAsync<int>(sql, new { PlayerId = playerId });
        }

        /// <summary>
        /// Changes the points of a player asynchronously.
        /// </summary>
        /// <param name="playerId">The player ID.</param>
        /// <param name="points">The points to be changed.</param>
        /// <returns>The task representing the asynchronous operation.</returns>
        public Task<int> ChangePointsAsync(string playerId, int points, bool allowNegative = false)
        {
            string sql;
            if (points >= 0 || allowNegative)
            {
                sql = $"{SqlGenerator.GetInsertStatement()} ON CONFLICT({nameof(T_PointsInfo.Id)}) DO UPDATE SET Points=Points+@Points";
            }
            else
            {
                sql = $"UPDATE {SqlGenerator.TableName} SET Points=Points+@Points WHERE Id=@Id AND Points>=@MinRequired";
            }

            var entity = new T_PointsInfo()
            {
                CreatedAt = DateTime.Now,
                Id = playerId,
                LastSignInAt = null,
                PlayerName = string.Empty,
                Points = points
            };

            if (points >= 0 || allowNegative)
            {
                return base.ExecuteAsync(sql, entity);
            }
            else
            {
                return base.ExecuteAsync(sql, new { Id = playerId, Points = points, MinRequired = -points });
            }
        }

        /// <summary>
        /// Gets a paged list of points information asynchronously.
        /// </summary>
        /// <param name="dto">The pagination query DTO.</param>
        /// <returns>A paged list of points information.</returns>
        public Task<PagedDto<T_PointsInfo>> GetPagedListAsync(PaginationQueryDto dto)
        {
            var whereClauseSB = new StringBuilder("1=1");

            if (string.IsNullOrEmpty(dto.Keyword) == false)
            {
                whereClauseSB.Append(" AND (Id=@Keyword OR PlayerName LIKE '%'||@Keyword||'%')");
            }

            var param = new { Keyword = dto.Keyword };
            return GetPagedListAsync(dto.PageNumber, dto.PageSize, whereClauseSB.ToString(), orderByClause: "PlayerName ASC", param);
        }

        /// <summary>
        /// Resets the points of all players asynchronously.
        /// </summary>
        /// <returns>The task representing the asynchronous operation.</returns>
        public Task<int> ResetPointsAsync()
        {
            string sql = $"UPDATE {SqlGenerator.TableName} SET Points=0";
            return base.ExecuteAsync(sql, useTransaction: true);
        }

        /// <summary>
        /// Resets the sign-in status of all players asynchronously.
        /// </summary>
        /// <returns>The task representing the asynchronous operation.</returns>
        public Task<int> ResetSignInAsync()
        {
            string sql = $"UPDATE {SqlGenerator.TableName} SET LastSignInAt=NULL";
            return base.ExecuteAsync(sql, useTransaction: true);
        }

        /// <summary>
        /// Gets the points of multiple players by their IDs asynchronously.
        /// </summary>
        /// <param name="playerIds">The player IDs.</param>
        /// <returns>A dictionary containing the player IDs and their corresponding points.</returns>
        public async Task<IReadOnlyDictionary<string, int>> GetPointsByIdsAsync(IEnumerable<string> playerIds)
        {
            string sql = $"SELECT Id,Points FROM {SqlGenerator.TableName} WHERE Id IN @PlayerIds";
            return (await base.ExecuteQueryAsync<(string Id, int Points)>(sql, new { PlayerIds = playerIds })).ToDictionary(k => k.Id, v => v.Points);
        }
    }
}