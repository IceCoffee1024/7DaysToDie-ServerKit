namespace SdtdServerKit.Data.Dtos
{
    /// <summary>
    /// 抽奖物品带权重DTO
    /// </summary>
    public class LotteryItemWithWeightDto
    {
        /// <summary>
        /// 物品Id
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// 权重（概率）
        /// </summary>
        public int Weight { get; set; }
    }
}
