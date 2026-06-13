namespace SdtdServerKit.Variables
{
    public class VipGiftVariables : VariablesBase
    {
        public required string GiftName { get; set; }

        public int TotalClaimCount { get; set; }

        public string? GiftDescription { get; set; }
    }
}