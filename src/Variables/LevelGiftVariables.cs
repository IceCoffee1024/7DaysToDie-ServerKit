namespace SdtdServerKit.Variables
{
    public class LevelGiftVariables : VariablesBase
    {
        public required string GiftName { get; set; }

        public int RequiredLevel { get; set; }

        public int TotalClaimCount { get; set; }

        public string? GiftDescription { get; set; }
    }
}
