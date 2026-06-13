namespace SdtdServerKit.Variables
{
    public class TeleportFriendVariables : VariablesBase
    {
        /// <summary>
        /// 传送间隔
        /// </summary>
        public int TeleInterval { get; set; }

        /// <summary>
        /// 需要积分
        /// </summary>
        public int PointsRequired { get; set; }

        /// <summary>
        /// 目标名称
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// 冷却时间
        /// </summary>
        public int CooldownSeconds { get; set; }
    }
}