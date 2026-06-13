using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// 抽奖命令关联表（命令奖品，带权重）
    /// </summary>
    [Table("T_LotteryCommand_v1")]
    public class T_LotteryCommand
    {
        /// <summary>
        /// 抽奖Id
        /// </summary>
        [PrimaryKey]
        public int LotteryId { get; set; }

        /// <summary>
        /// 命令Id
        /// </summary>
        [PrimaryKey]
        public int CommandId { get; set; }

        /// <summary>
        /// 权重（概率）
        /// </summary>
        public int Weight { get; set; }
    }
}
