using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 等级礼包物品仓储
    /// </summary>
    public class LevelGiftItemRepository : DefaultRepository<T_LevelGiftItem>, ILevelGiftItemRepository
    {
        public Task<int> DeleteByLevelGiftIdAsync(string levelGiftId)
        {
            string whereClause = "LevelGiftId=@LevelGiftId";
            return base.DeleteAsync(whereClause, param: new { LevelGiftId = levelGiftId }, useTransaction: true);
        }
    }
}
