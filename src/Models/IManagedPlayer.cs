namespace SdtdServerKit.Models
{
    /// <summary>
    /// Represents a managed player.
    /// </summary>
    public interface IManagedPlayer : IPlayerBase
    {
        /// <summary>
        /// Gets the persistent player data.
        /// </summary>
        [JsonIgnore]
        PersistentPlayerData PersistentPlayerData { get; }

        /// <summary>
        /// Gets the player data file.
        /// </summary>
        [JsonIgnore]
        PlayerDataFile PlayerDataFile { get; }

        /// <summary>
        /// Gets the client information.
        /// </summary>
        [JsonIgnore]
        ClientInfo ClientInfo { get; }

        /// <summary>
        /// Gets the entity player.
        /// </summary>
        [JsonIgnore]
        EntityPlayer EntityPlayer { get; }
    }
}
