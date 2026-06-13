using IceCoffee.SimpleCRUD.Dtos;
using IceCoffee.SimpleCRUD.SqlGenerators;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System.Text;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public class ItemListRepository : DefaultRepository<T_ItemList>, IItemListRepository
    {
        public Task InsertWithIdAsync(T_ItemList entity)
        {
            string sql = $"INSERT INTO {SqlGenerator.TableName} (Id, CreatedAt, DisplayName, RewardType, ItemName, [Count], Quality, Durability, Description) VALUES (@Id, @CreatedAt, @DisplayName, @RewardType, @ItemName, @Count, @Quality, @Durability, @Description)";
            return base.ExecuteAsync(sql, entity);
        }
        public Task<IEnumerable<T_ItemList>> GetListByCdKeyIdAsync(int cdKeyId)
        {
            string whereClause = "Id IN (SELECT ItemId FROM CdKeyItem WHERE CdKeyId=@CdKeyId)";
            return base.GetListAsync(whereClause, param: new { CdKeyId = cdKeyId });
        }

        /// <inheritdoc/>
        public Task<IEnumerable<T_ItemList>> GetListByGoodsIdAsync(int goodsId)
        {
            string whereClause = "Id IN (SELECT ItemId FROM T_GoodsItem WHERE GoodsId=@GoodsId)";
            return base.GetListAsync(whereClause, param: new { GoodsId = goodsId });
        }

        /// <inheritdoc/>
        public Task<IEnumerable<T_ItemList>> GetListByVipGiftIdAsync(string vipGiftId)
        {
            string tableName = GetSqlGenerator<T_VipGiftItem>().TableName;
            string whereClause = $"Id IN (SELECT ItemId FROM {tableName} WHERE VipGiftId=@VipGiftId)";
            return base.GetListAsync(whereClause, param: new { VipGiftId = vipGiftId });
        }

        /// <inheritdoc/>
        public Task<IEnumerable<T_ItemList>> GetListByLevelGiftIdAsync(string levelGiftId)
        {
            string tableName = GetSqlGenerator<T_LevelGiftItem>().TableName;
            string whereClause = $"Id IN (SELECT ItemId FROM {tableName} WHERE LevelGiftId=@LevelGiftId)";
            return base.GetListAsync(whereClause, param: new { LevelGiftId = levelGiftId });
        }

        public Task<PagedDto<T_ItemList>> GetPagedListAsync(PaginationQueryDto dto)
        {
            var whereClauseSB = new StringBuilder("RewardType = 0");

            if (string.IsNullOrEmpty(dto.Keyword) == false)
            {
                whereClauseSB.Append(" AND (Id=@Keyword OR ItemName LIKE '%'||@Keyword||'%' OR DisplayName LIKE '%'||@Keyword||'%' OR Description LIKE '%'||@Keyword||'%')");
            }

            var param = new { Keyword = dto.Keyword };
            return GetPagedListAsync(dto.PageNumber, dto.PageSize, whereClauseSB.ToString(), orderByClause: "Id DESC", param);
        }
    }
}