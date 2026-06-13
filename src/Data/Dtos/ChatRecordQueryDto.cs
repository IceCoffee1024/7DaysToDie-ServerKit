using IceCoffee.SimpleCRUD.Dtos;

namespace SdtdServerKit.Data.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    public class ChatRecordQueryDto : PaginationQueryDto<ChatRecordQueryOrder>
    {
        /// <summary>
        /// Start Date Time
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// End Date Time
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Chat Type
        /// </summary>
        public ChatType? ChatType { get; set; }

        /// <summary>
        /// 排除系统消息
        /// </summary>
        public bool? ExcludeSystemMessages { get; set; }
    }
}
