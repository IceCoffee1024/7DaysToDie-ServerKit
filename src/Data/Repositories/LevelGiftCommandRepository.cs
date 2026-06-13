using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 等级礼包命令仓储
    /// </summary>
    public class LevelGiftCommandRepository : DefaultRepository<T_LevelGiftCommand>, ILevelGiftCommandRepository
    {
        public Task<int> DeleteByLevelGiftIdAsync(string levelGiftId)
        {
            string whereClause = "LevelGiftId=@LevelGiftId";
            return base.DeleteAsync(whereClause, param: new { LevelGiftId = levelGiftId }, useTransaction: true);
        }
    }
}
