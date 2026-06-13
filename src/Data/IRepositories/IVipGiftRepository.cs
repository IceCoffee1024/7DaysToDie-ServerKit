using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// Gift repository
    /// </summary>
    public interface IVipGiftRepository : IRepository<T_VipGift>
    {
        /// <summary>
        /// Reset claim state
        /// </summary>
        /// <returns></returns>
        Task<int> ResetClaimStateAsync();
    }
}