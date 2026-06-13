namespace SdtdServerKit.Models
{
    /// <summary>
    /// 等级礼包
    /// </summary>
    public class LevelGift
    {
        /// <summary>
        /// 玩家Id（玩家礼包）或礼包唯一ID（通用礼包）
        /// </summary>
        public string Id { get; set; }

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
        /// 领取状态
        /// </summary>
        public bool ClaimState { get; set; }

        /// <summary>
        /// 总领取次数
        /// </summary>
        public int TotalClaimCount { get; set; }

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
