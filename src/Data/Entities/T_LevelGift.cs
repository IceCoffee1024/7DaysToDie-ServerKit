using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// 等级礼包
    /// </summary>
    [Table("T_LevelGift_v1")]
    public class T_LevelGift
    {
        /// <summary>
        /// 玩家Id（玩家礼包）或礼包唯一ID（通用礼包）
        /// </summary>
        [PrimaryKey]
        public string Id { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [IgnoreUpdate]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 礼包名称
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 玩家名称（仅玩家礼包使用）
        /// </summary>
        public string? PlayerName { get; set; }

        /// <summary>
        /// 等级要求
        /// </summary>
        public int RequiredLevel { get; set; }

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

        /// <summary>
        /// 礼包类型：0=玩家礼包，1=通用礼包
        /// </summary>
        public int GiftType { get; set; }
    }
}
