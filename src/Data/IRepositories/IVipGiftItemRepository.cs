using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 
    /// </summary>
    public interface IVipGiftItemRepository : IRepository<T_VipGiftItem>
    {
        /// <summary>
        /// 通过礼品Id删除记录
        /// </summary>
        /// <param name="vipGiftId"></param>
        /// <returns></returns>
        Task<int> DeleteByVipGiftIdAsync(string vipGiftId);
    }
}