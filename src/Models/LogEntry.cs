namespace SdtdServerKit.Models
{
    /// <summary>
    /// 日志条目
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// 日志类型
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }
}