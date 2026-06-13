namespace SdtdServerKit.Models
{
    /// <summary>
    /// 日志级别
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LogLevel
    {
        /// <summary>
        /// 错误
        /// </summary>
        Error,

        /// <summary>
        /// 断言
        /// </summary>
        Assert,

        /// <summary>
        /// 警告
        /// </summary>
        Warning,

        /// <summary>
        /// 记录
        /// </summary>
        Log,

        /// <summary>
        /// 异常
        /// </summary>
        Exception
    }
}