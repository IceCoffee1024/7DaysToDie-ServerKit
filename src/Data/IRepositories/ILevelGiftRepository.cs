using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 等级礼包仓储接口
    /// </summary>
    public interface ILevelGiftRepository : IRepository<T_LevelGift>
    {
        /// <summary>
        /// 重置领取状态
        /// </summary>
        /// <returns></returns>
        Task<int> ResetClaimStateAsync();
    }
}
