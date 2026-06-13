using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// 商品
    /// </summary>
    [Table("T_VipGift_v1")]
    public class T_VipGift
    {
        /// <summary>
        /// 玩家Id
        /// </summary>
        [PrimaryKey]
        public string Id { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [IgnoreUpdate]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 玩家名称
        /// </summary>
        public string? PlayerName { get; set; }

        /// <summary>
        /// 领取状态, true: 已领取, false: 未领取
        /// </summary>
        public bool ClaimState { get; set; }

        /// <summary>
        /// 总领取次数
        /// </summary>
        public int TotalClaimCount { get; set; }

        /// <summary>
        /// 最后领取日期
        /// </summary>
        public DateTime? LastClaimAt { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string? Description { get; set; }
    }
}