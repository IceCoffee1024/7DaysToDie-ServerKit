namespace SdtdServerKit.Variables
{
    public class TeleportHomeVariables : VariablesBase
    {
        /// <summary>
        /// Home名称
        /// </summary>
        public string HomeName { get; set; }

        /// <summary>
        /// 传送间隔
        /// </summary>
        public int TeleInterval { get; set; }

        /// <summary>
        /// 冷却时间
        /// </summary>
        public int CooldownSeconds { get; set; }

    }
}