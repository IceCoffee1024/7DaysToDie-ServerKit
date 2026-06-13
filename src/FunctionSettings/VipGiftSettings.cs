namespace SdtdServerKit.FunctionSettings
{
    public class VipGiftSettings : SettingsBase
    {
        /// <summary>
        /// Claim Cmd
        /// </summary>
        public required string ClaimCmd { get; set; }

        /// <summary>
        /// Has Claimed Tip
        /// </summary>
        public required string HasClaimedTip { get; set; }

        /// <summary>
        /// Non Vip Tip
        /// </summary>
        public required string NonVipTip { get; set; }

        /// <summary>
        /// Claim Success Tip
        /// </summary>
        public required string ClaimSuccessTip { get; set; }
    }
}
