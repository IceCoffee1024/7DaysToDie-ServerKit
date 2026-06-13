using System.ComponentModel;

namespace SdtdServerKit.Models
{
    /// <summary>
    /// Represents the base information of a player.
    /// </summary>
    public class PlayerBase : IPlayerBase
    {
        internal PlayerBase() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerBase"/> class.
        /// </summary>
        public PlayerBase(string playerId, string playerName, int entityId, string platformId)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            EntityId = entityId;
            PlatformId = platformId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerBase"/> class.
        /// </summary>
        /// <param name="playerBase"></param>
        public PlayerBase(IPlayerBase playerBase)
        {
            PlayerId = playerBase.PlayerId;
            PlayerName = playerBase.PlayerName;
            EntityId = playerBase.EntityId;
            PlatformId = playerBase.PlatformId;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual string PlayerId { get; set; } = null!;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual string PlayerName { get; set; } = null!;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [DefaultValue(-1)]
        public virtual int EntityId { get; set; } = -1;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual string PlatformId { get; set; } = null!;
    }
}
