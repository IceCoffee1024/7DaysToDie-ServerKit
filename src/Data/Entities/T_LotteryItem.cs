using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// 抽奖物品关联表
    /// </summary>
    [Table("T_LotteryItem_v1")]
    public class T_LotteryItem
    {
        /// <summary>
        /// 抽奖Id
        /// </summary>
        [PrimaryKey]
        public int LotteryId { get; set; }

        /// <summary>
        /// 物品Id
        /// </summary>
        [PrimaryKey]
        public int ItemId { get; set; }

        /// <summary>
        /// 权重（概率）
        /// </summary>
        public int Weight { get; set; }
    }
}
