using SdtdServerKit.Data;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 城市位置仓储
    /// </summary>
    public class CityLocationRepository : DefaultRepository<T_CityLocation>, ICityLocationRepository
    {
        /// <inheritdoc/>
        public Task<T_CityLocation?> GetByNameAsync(string name)
        {
            return base.GetFirstOrDefaultAsync("CityName=@CityName", param: new { CityName = name });
        }
    }
}