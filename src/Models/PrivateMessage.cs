namespace SdtdServerKit.Models
{
    public class PrivateMessage : GlobalMessage
    {
        /// <summary>
        /// 目标玩家的Id或昵称
        /// </summary>
        public required string TargetPlayerIdOrName { get; set; }
    }
}