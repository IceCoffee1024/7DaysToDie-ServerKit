namespace SdtdServerKit.Data.Dtos
{
    /// <summary>
    /// 抽奖命令奖品带权重DTO
    /// </summary>
    public class LotteryCommandWithWeightDto
    {
        /// <summary>
        /// 命令Id
        /// </summary>
        public int CommandId { get; set; }

        /// <summary>
        /// 权重（概率）
        /// </summary>
        public int Weight { get; set; }
    }
}
