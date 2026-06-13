namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// 抽奖命令奖品（带权重）
    /// </summary>
    public class LotteryCommandWithWeight
    {
        public int Id { get; set; }
        public string? DisplayName { get; set; }
        public string Command { get; set; } = string.Empty;
        public bool InMainThread { get; set; }
        public string? Description { get; set; }
        public int Weight { get; set; }
    }
}
