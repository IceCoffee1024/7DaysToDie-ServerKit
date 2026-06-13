using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 等级礼包物品仓储接口
    /// </summary>
    public interface ILevelGiftItemRepository : IRepository<T_LevelGiftItem>
    {
        /// <summary>
        /// 根据礼包ID删除物品
        /// </summary>
        /// <param name="levelGiftId"></param>
        /// <returns></returns>
        Task<int> DeleteByLevelGiftIdAsync(string levelGiftId);
    }
}
