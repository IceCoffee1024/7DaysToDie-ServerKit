namespace SdtdServerKit.Data.Dtos
{
    /// <summary>
    /// 抽奖DTO
    /// </summary>
    public class LotteryDto
    {
        /// <summary>
        /// 唯一Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 抽奖池名称
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 抽奖命令
        /// </summary>
        public string? DrawCommand { get; set; }

        /// <summary>
        /// 抽奖间隔（秒）
        /// </summary>
        public int DrawInterval { get; set; }

        /// <summary>
        /// 单次抽奖消耗积分
        /// </summary>
        public int DrawCost { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string? Description { get; set; }
    }
}
