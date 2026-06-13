namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// 抽奖物品（带权重）
    /// </summary>
    public class LotteryItemWithWeight
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        /// <summary>
        /// 奖品类型 0=物品 2=积分
        /// </summary>
        public int RewardType { get; set; }
        public int Count { get; set; }
        public int Quality { get; set; }
        public int Durability { get; set; }
        public int Weight { get; set; }
    }
}
