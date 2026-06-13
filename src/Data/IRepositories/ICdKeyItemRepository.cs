using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// Represents a Cd Key Item repository interface.
    /// </summary>
    public interface ICdKeyItemRepository : IRepository<CdKeyItem>
    {
        /// <summary>
        /// Delete records by CdKeyId.
        /// </summary>
        /// <param name="cdKeyId"></param>
        /// <returns></returns>
        Task<int> DeleteByCdKeyIdAsync(int cdKeyId);
    }
}