using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// 等级礼包命令仓储接口
    /// </summary>
    public interface ILevelGiftCommandRepository : IRepository<T_LevelGiftCommand>
    {
        /// <summary>
        /// 根据礼包ID删除命令
        /// </summary>
        /// <param name="levelGiftId"></param>
        /// <returns></returns>
        Task<int> DeleteByLevelGiftIdAsync(string levelGiftId);
    }
}
