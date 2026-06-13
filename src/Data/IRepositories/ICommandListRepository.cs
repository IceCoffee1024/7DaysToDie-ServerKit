using IceCoffee.SimpleCRUD;
using IceCoffee.SimpleCRUD.Dtos;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    /// <summary>
    /// Command list repository
    /// </summary>
    public interface ICommandListRepository : IRepository<T_CommandList>
    {
        /// <summary>
        /// 使用显式ID插入
        /// </summary>
        Task InsertWithIdAsync(T_CommandList entity);

        /// <summary>
        /// Get command list by goods Id
        /// </summary>
        /// <param name="goodsId">The goods Id</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the command list.</returns>
        Task<IEnumerable<T_CommandList>> GetListByGoodsIdAsync(int goodsId);

        /// <summary>
        /// Get command list by VIP gift Id
        /// </summary>
        /// <param name="vipGiftId">The VIP gift Id</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the command list.</returns>
        Task<IEnumerable<T_CommandList>> GetListByVipGiftIdAsync(string vipGiftId);

        /// <summary>
        /// Get command list by task schedule Id
        /// </summary>
        /// <param name="taskScheduleId">The task schedule Id</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the command list.</returns>
        Task<IEnumerable<T_CommandList>> GetListByTaskScheduleIdAsync(int taskScheduleId);

        /// <summary>
        /// Get paged command list
        /// </summary>
        /// <param name="dto">The pagination query DTO</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the paged command list.</returns>
        Task<PagedDto<T_CommandList>> GetPagedListAsync(PaginationQueryDto dto);

        /// <summary>
        /// Get command list by CdKey Id
        /// </summary>
        /// <param name="cdKeyId">The CdKey Id</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the command list.</returns>
        Task<IEnumerable<T_CommandList>> GetListByCdKeyIdAsync(int cdKeyId);

        /// <summary>
        /// 获得命令列表的等级礼包id
        /// </summary>
        Task<IEnumerable<T_CommandList>> GetListByLevelGiftIdAsync(string levelGiftId);
    }
}