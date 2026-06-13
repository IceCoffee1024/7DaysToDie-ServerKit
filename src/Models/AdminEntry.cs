namespace SdtdServerKit.Models
{
    /// <summary>
    /// 管理员条目
    /// </summary>
    public class AdminEntry
    {
        /// <summary>
        /// 玩家Id（平台Id或跨平台Id, 格式：平台类型 + Id, 如 EOS_XXXX 或 Steam_XXXX）
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// 权限等级
        /// </summary>
        public int PermissionLevel { get; set; }

        /// <summary>
        /// 显示名称, 默认为玩家昵称
        /// </summary>
        public string DisplayName { get; set; }
    }
}