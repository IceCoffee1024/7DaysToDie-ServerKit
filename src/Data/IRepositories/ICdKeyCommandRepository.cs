using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// Represents a Cd Key Command repository interface.
    /// </summary>
    public interface ICdKeyCommandRepository : IRepository<CdKeyCommand>
    {
        /// <summary>
        /// Delete records by CdKeyId.
        /// </summary>
        /// <param name="cdKeyId"></param>
        /// <returns></returns>
        Task<int> DeleteByCdKeyIdAsync(int cdKeyId);
    }
}