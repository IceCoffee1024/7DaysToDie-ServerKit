namespace SdtdServerKit.Models
{
    public class AllowedCommand
    {
        /// <summary>
        /// 命令
        /// </summary>
        public IEnumerable<string> Commands { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 帮助
        /// </summary>
        public string Help { get; set; }

        /// <summary>
        /// 权限等级
        /// </summary>
        public int PermissionLevel { get; set; }
    }
}