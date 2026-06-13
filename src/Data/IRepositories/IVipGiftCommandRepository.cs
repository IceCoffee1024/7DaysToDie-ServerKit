using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 
    /// </summary>
    public interface IVipGiftCommandRepository : IRepository<T_VipGiftCommand>
    {
        /// <summary>
        /// 通过礼品Id删除记录
        /// </summary>
        /// <param name="vipGiftId"></param>
        /// <returns></returns>
        Task<int> DeleteByVipGiftIdAsync(string vipGiftId);

    }
}