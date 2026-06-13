namespace SdtdServerKit.Models
{
    /// <summary>
    /// Represents a player's basic infomation.
    /// </summary>
    public interface IPlayerBase
    {
        /// <summary>
        /// Gets the player cross-platform ID (EOS).
        /// </summary>
        string PlayerId { get; }

        /// <summary>
        /// Gets the player name.
        /// </summary>
        string PlayerName { get; }

        /// <summary>
        /// Gets the entity ID.
        /// </summary>
        int EntityId { get; }

        /// <summary>
        /// Gets the platform ID.
        /// </summary>
        string PlatformId { get; }
    }
}
