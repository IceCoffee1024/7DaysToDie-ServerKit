using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 抽奖命令关联仓储
    /// </summary>
    public class LotteryCommandRepository : DefaultRepository<T_LotteryCommand>, ILotteryCommandRepository
    {
        public Task<int> DeleteByLotteryIdAsync(int lotteryId)
        {
            return base.DeleteAsync("LotteryId=@LotteryId", param: new { LotteryId = lotteryId });
        }

        public Task<IEnumerable<T_LotteryCommand>> GetListByLotteryIdAsync(int lotteryId)
        {
            return base.GetListAsync("LotteryId=@LotteryId", param: new { LotteryId = lotteryId });
        }

        public Task<IEnumerable<LotteryCommandWithWeight>> GetCommandsWithWeightByLotteryIdAsync(int lotteryId)
        {
            string cmdTable = GetSqlGenerator<T_CommandList>().TableName;
            string sql = $@"SELECT c.Id, c.DisplayName, c.Command, c.InMainThread, c.Description, lc.Weight
                FROM {cmdTable} c
                INNER JOIN {SqlGenerator.TableName} lc ON c.Id = lc.CommandId
                WHERE lc.LotteryId = @LotteryId";
            return base.ExecuteQueryAsync<LotteryCommandWithWeight>(sql, new { LotteryId = lotteryId });
        }
    }
}
