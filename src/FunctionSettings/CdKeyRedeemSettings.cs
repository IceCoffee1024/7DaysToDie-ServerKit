namespace SdtdServerKit.FunctionSettings
{
    public class CdKeyRedeemSettings : SettingsBase
    {
        /// <summary>
        /// Has Already Redeemed Tip
        /// </summary>
        public required string HasAlreadyRedeemedTip { get; set; }

        /// <summary>
        /// Has Reached Max Redemption Limit Tip
        /// </summary>
        public required string HasReachedMaxRedemptionLimitTip { get; set; }

        /// <summary>
        /// Has Redemption Code Expired
        /// </summary>
        public required string HasRedemptionCodeExpiredTip { get; set; }

        /// <summary>
        /// Redeem Success Tip
        /// </summary>
        public required string RedeemSuccessTip { get; set; }
    }
}
