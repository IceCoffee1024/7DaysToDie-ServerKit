namespace SdtdServerKit.Variables
{
    /// <summary>
    /// 抽奖变量
    /// </summary>
    public class LotteryVariables : VariablesBase
    {
        /// <summary>
        /// 抽奖池ID
        /// </summary>
        public int LotteryId { get; set; }

        /// <summary>
        /// 抽奖池名称
        /// </summary>
        public string LotteryName { get; set; } = string.Empty;

        /// <summary>
        /// 单次抽奖消耗积分
        /// </summary>
        public int DrawCost { get; set; }

        /// <summary>
        /// 抽奖间隔（秒）
        /// </summary>
        public int DrawInterval { get; set; }

        /// <summary>
        /// 当前积分
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// 剩余冷却时间（秒）
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// 抽中的显示名称（优先 DisplayName，没有则回退到物品本地化名 / 命令文本）
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// 抽中的物品数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 抽中的物品质量
        /// </summary>
        public int Quality { get; set; }

        /// <summary>
        /// 抽中的物品耐久度
        /// </summary>
        public int Durability { get; set; }
    }
}
