using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 抽奖物品关联仓储
    /// </summary>
    public class LotteryItemRepository : DefaultRepository<T_LotteryItem>, ILotteryItemRepository
    {
        public Task<int> DeleteByLotteryIdAsync(int lotteryId)
        {
            return base.DeleteAsync("LotteryId=@LotteryId", param: new { LotteryId = lotteryId });
        }

        public Task<IEnumerable<T_LotteryItem>> GetListByLotteryIdAsync(int lotteryId)
        {
            return base.GetListAsync("LotteryId=@LotteryId", param: new { LotteryId = lotteryId });
        }

        public Task<IEnumerable<LotteryItemWithWeight>> GetItemsWithWeightByLotteryIdAsync(int lotteryId)
        {
            string itemTable = GetSqlGenerator<T_ItemList>().TableName;
            string sql = $@"SELECT i.Id, i.ItemName, i.DisplayName, i.RewardType, i.[Count], i.Quality, i.Durability, li.Weight
                FROM {itemTable} i
                INNER JOIN {SqlGenerator.TableName} li ON i.Id = li.ItemId
                WHERE li.LotteryId = @LotteryId";
            return base.ExecuteQueryAsync<LotteryItemWithWeight>(sql, new { LotteryId = lotteryId });
        }
    }
}
