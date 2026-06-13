using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 城市位置仓储
    /// </summary>
    public interface ICityLocationRepository : IRepository<T_CityLocation>
    {
        /// <summary>
        /// 根据名称获取城市位置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<T_CityLocation?> GetByNameAsync(string name);
    }
}