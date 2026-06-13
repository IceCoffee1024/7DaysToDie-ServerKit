using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// 区域命令禁用仓储实现
    /// </summary>
    public class MuteCommandAreaRepository : DefaultRepository<T_MuteCommandArea>, IMuteCommandAreaRepository
    {
        public Task<IEnumerable<T_MuteCommandArea>> GetAllAsync()
        {
            return base.GetListAsync();
        }

        public Task<int> DeleteByAreaAndCommandAsync(int minX, int minZ, int maxX, int maxZ, string command)
        {
            return base.DeleteAsync(
                "MinX=@MinX AND MinZ=@MinZ AND MaxX=@MaxX AND MaxZ=@MaxZ AND Command=@Command",
                param: new { MinX = minX, MinZ = minZ, MaxX = maxX, MaxZ = maxZ, Command = command });
        }

        public Task<int> DeleteByAreaAsync(int minX, int minZ, int maxX, int maxZ)
        {
            return base.DeleteAsync(
                "MinX=@MinX AND MinZ=@MinZ AND MaxX=@MaxX AND MaxZ=@MaxZ",
                param: new { MinX = minX, MinZ = minZ, MaxX = maxX, MaxZ = maxZ });
        }

        public Task<int> DeleteAllAsync()
        {
            return base.DeleteAsync("1=1");
        }
    }
}
