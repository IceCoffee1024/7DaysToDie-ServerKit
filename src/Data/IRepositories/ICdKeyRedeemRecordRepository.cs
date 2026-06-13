using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// Represents a Cd Key Redeem Record repository interface.
    /// </summary>
    public interface ICdKeyRedeemRecordRepository : IRepository<CdKeyRedeemRecord>
    {
        /// <summary>
        /// Get a Cd Key Redeem Record by key and player id.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="playerId"></param>
        /// <returns></returns>
        Task<CdKeyRedeemRecord?> GetByKeyAndPlayerIdAsync(string key, string playerId);
    }
}