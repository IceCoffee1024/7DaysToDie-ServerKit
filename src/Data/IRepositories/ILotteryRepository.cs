using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 抽奖仓储
    /// </summary>
    public interface ILotteryRepository : IRepository<T_Lottery>
    {
        /// <summary>
        /// 获取所有抽奖配置按Id升序排序
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<T_Lottery>> GetAllOrderByIdAsync();
    }
}
