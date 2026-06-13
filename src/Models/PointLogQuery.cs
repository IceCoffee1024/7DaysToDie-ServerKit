namespace SdtdServerKit.Models
{
    /// <summary>
    /// 积分日志列表排序字段
    /// </summary>
    public enum PointLogQueryOrder
    {
        /// <summary>创建时间</summary>
        CreatedAt,

        /// <summary>变动积分</summary>
        Spend,

        /// <summary>操作后余额</summary>
        Balance,
    }

    /// <summary>
    /// 积分日志查询参数（URI）
    /// </summary>
    public class PointLogQuery : PaginationQuery<PointLogQueryOrder>
    {
        /// <summary>开始日期</summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>结束日期</summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>日志分类筛选（不传 = 全部）</summary>
        public PointLogCategory? Category { get; set; }

        /// <summary>变化类型筛选（不传 = 全部）</summary>
        public PointChangeType? ChangeType { get; set; }
    }
}
