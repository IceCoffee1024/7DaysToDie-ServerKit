using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// 抽奖记录
    /// </summary>
    [Table("T_LotteryRecord_v1")]
    public class T_LotteryRecord
    {
        /// <summary>
        /// 玩家Id
        /// </summary>
        [PrimaryKey]
        public required string PlayerId { get; set; }

        /// <summary>
        /// 抽奖Id
        /// </summary>
        [PrimaryKey]
        public int LotteryId { get; set; }

        /// <summary>
        /// 玩家名称
        /// </summary>
        public string? PlayerName { get; set; }

        /// <summary>
        /// 抽奖时间
        /// </summary>
        [IgnoreUpdate]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 上次抽奖时间
        /// </summary>
        public DateTime? LastDrawAt { get; set; }
    }
}
