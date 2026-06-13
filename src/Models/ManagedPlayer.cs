namespace SdtdServerKit.Models
{
    /// <summary>
    /// Represents a managed player in the game.
    /// </summary>
    public class ManagedPlayer : PlayerBase, IManagedPlayer
    {
        private readonly ClientInfo _clientInfo;
        private readonly PersistentPlayerData _persistentPlayerData;
        private readonly EntityPlayer _entityPlayer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedPlayer"/> class.
        /// </summary>
        /// <param name="clientInfo">The client information.</param>
        public ManagedPlayer(ClientInfo clientInfo)
        {
            _clientInfo = clientInfo;
            _persistentPlayerData = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(clientInfo.entityId);
            _entityPlayer = GameManager.Instance.World.Players.dict[clientInfo.entityId];
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public EntityPlayer EntityPlayer => _entityPlayer;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string PlayerId => _clientInfo.InternalId.CombinedString;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string PlayerName => _clientInfo.playerName;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override int EntityId => _clientInfo.entityId;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string PlatformId => _clientInfo.PlatformId.CombinedString;
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public PersistentPlayerData PersistentPlayerData => _persistentPlayerData;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public PlayerDataFile PlayerDataFile => _clientInfo.latestPlayerData;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ClientInfo ClientInfo => _clientInfo;
    }
}