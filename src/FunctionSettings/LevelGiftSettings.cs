namespace SdtdServerKit.FunctionSettings
{
    public class LevelGiftSettings : SettingsBase
    {
        /// <summary>
        /// 领取命令
        /// </summary>
        public required string ClaimCmd { get; set; }

        /// <summary>
        /// 已领取提示
        /// </summary>
        public required string HasClaimedTip { get; set; }

        /// <summary>
        /// 等级不足提示
        /// </summary>
        public required string LevelNotEnoughTip { get; set; }

        /// <summary>
        /// 无礼包提示
        /// </summary>
        public required string NoGiftTip { get; set; }

        /// <summary>
        /// 领取成功提示
        /// </summary>
        public required string ClaimSuccessTip { get; set; }
    }
}
