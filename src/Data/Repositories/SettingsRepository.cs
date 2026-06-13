using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    public class SettingsRepository : DefaultRepository<T_Settings>, ISettingsRepository
    {
        public SettingsRepository()
        {
        }
    }
}