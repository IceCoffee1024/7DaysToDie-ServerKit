using IceCoffee.SimpleCRUD.Dtos;
using SdtdServerKit.Models;

namespace SdtdServerKit.Data.Dtos
{
    /// <summary>
    /// 积分日志分页查询 DTO
    /// </summary>
    public class PointLogQueryDto : PaginationQueryDto<PointLogQueryOrder>
    {
        /// <summary>开始日期</summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>结束日期</summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>日志分类</summary>
        public PointLogCategory? Category { get; set; }

        /// <summary>变化类型</summary>
        public PointChangeType? ChangeType { get; set; }
    }
}
