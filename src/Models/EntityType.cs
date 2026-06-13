namespace SdtdServerKit.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntityType
    {
        OfflinePlayer,
        OnlinePlayer = 1,
        Zombie,
        Animal,
        Bandit,
        Vehicle,

        // group
        Hostiles = -1
    }
}