using IceCoffee.SimpleCRUD;
using IceCoffee.SimpleCRUD.Dtos;
using SdtdServerKit.Data.Dtos;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Data.IRepositories
{
    public interface IChatRecordRepository : IRepository<T_ChatRecord>
    {
        Task<PagedDto<T_ChatRecord>> GetPagedListAsync(ChatRecordQueryDto dto);
    }
}