using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 商品仓储
    /// </summary>
    public interface IGoodsRepository : IRepository<T_Goods>
    {
        /// <summary>
        /// 获取所有商品按Id升序排序
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<T_Goods>> GetAllOrderByIdAsync();
    }
}