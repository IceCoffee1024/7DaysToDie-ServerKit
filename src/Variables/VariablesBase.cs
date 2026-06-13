namespace SdtdServerKit.Variables
{
    /// <summary>
    /// 变量基类
    /// </summary>
    public class VariablesBase
    {
        /// <summary>
        /// 玩家Id
        /// </summary>
        public string? PlayerId { get; set; }

        /// <summary>
        /// 玩家名称
        /// </summary>
        public string? PlayerName { get; set; }

        /// <summary>
        /// 实体Id
        /// </summary>
        public int EntityId { get; set; }
    }
}