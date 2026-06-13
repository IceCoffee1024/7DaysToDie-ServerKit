namespace SdtdServerKit.Models
{
    /// <summary>
    /// 黑名单条目
    /// </summary>
    public class BlacklistEntry
    {
        /// <summary>
        /// 解封日期
        /// </summary>
        public DateTime BannedUntil { get; set; }

        /// <summary>
        /// 显示名称, 默认为玩家昵称
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 玩家Id（平台Id或跨平台Id, 格式：平台类型 + Id, 如 EOS_XXXX 或 Steam_XXXX）
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// 封禁原因
        /// </summary>
        public string? Reason { get; set; }
    }
}