namespace SdtdServerKit.Models
{
    /// <summary>
    /// 聊天类型
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChatType
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// 全局
        /// </summary>
        Global = 0,

        /// <summary>
        /// 好友
        /// </summary>
        Friends = 1,

        /// <summary>
        /// 组队
        /// </summary>
        Party = 2,

        /// <summary>
        /// 私人
        /// </summary>
        Whisper = 3,
    }
}