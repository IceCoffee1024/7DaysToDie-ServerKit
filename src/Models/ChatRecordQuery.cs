using System.ComponentModel;

namespace SdtdServerKit.Models
{
    public enum ChatRecordQueryOrder
    {
        /// <summary>
        /// 
        /// </summary>
        CreatedAt,
    }

    /// <summary>
    /// Chat Record Query
    /// </summary>
    public class ChatRecordQuery : PaginationQuery<ChatRecordQueryOrder>
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