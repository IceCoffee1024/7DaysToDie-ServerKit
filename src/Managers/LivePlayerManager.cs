namespace SdtdServerKit.Managers
{
    /// <summary>
    /// Represents a collection of online players.
    /// </summary>
    public static class LivePlayerManager
    {
        private static readonly ThreadSafeReadDictionary<int, ManagedPlayer> _entityIdMap;
        private static readonly ThreadSafeReadDictionary<string, ManagedPlayer> _playerIdMap;

        static LivePlayerManager()
        {
            _entityIdMap = new ThreadSafeReadDictionary<int, ManagedPlayer>();
            _playerIdMap = new ThreadSafeReadDictionary<string, ManagedPlayer>();
        }

        /// <summary>
        /// Adds a new online player to the collection.
        /// </summary>
        /// <param name="clientInfo">The client information of the player.</param>
        /// <returns>The added online player.</returns>
        internal static ManagedPlayer Add(ClientInfo clientInfo)
        {
            var managedPlayer = new ManagedPlayer(clientInfo);
            _entityIdMap.Add(managedPlayer.EntityId, managedPlayer);
            _playerIdMap.Add(managedPlayer.PlayerId, managedPlayer);
            return managedPlayer;
        }

        /// <summary>
        /// Removes an online player from the collection.
        /// </summary>
        /// <param name="clientInfo">The client information of the player.</param>
        internal static void Remove(ClientInfo clientInfo)
        {
            _entityIdMap.Remove(clientInfo.entityId);
            _playerIdMap.Remove(clientInfo.InternalId.CombinedString);
        }

        /// <summary>
        /// Gets an online player by their entity ID.
        /// </summary>
        /// <param name="entityId">The entity ID of the player.</param>
        /// <returns>The online player with the specified entity ID, or throw exception if not found.</returns>
        public static ManagedPlayer GetByEntityId(int entityId)
        {
            return _entityIdMap.Read(entityId);
        }

        /// <summary>
        /// Gets an online player by their player ID.
        /// </summary>
        /// <param name="playerId">The player ID of the player.</param>
        /// <returns>The online player with the specified player ID, or throw exception if not found.</returns>
        public static ManagedPlayer GetByPlayerId(string playerId)
        {
            return _playerIdMap.Read(playerId);
        }

        /// <summary>
        /// Tries to get an online player by their entity ID.
        /// </summary>
        /// <param name="entityId">The entity ID of the player.</param>
        /// <param name="managedPlayer">The online player with the specified entity ID, if found; otherwise, null.</param>
        /// <returns>true if the online player with the specified entity ID is found; otherwise, false.</returns>
        public static bool TryGetByEntityId(int entityId, out ManagedPlayer? managedPlayer)
        {
            return _entityIdMap.TryRead(entityId, out managedPlayer);
        }

        /// <summary>
        /// Tries to get an online player by their player ID.
        /// </summary>
        /// <param name="playerId">The player ID of the player.</param>
        /// <param name="managedPlayer">The online player with the specified player ID, if found; otherwise, null.</param>
        /// <returns>true if the online player with the specified player ID is found; otherwise, false.</returns>
        public static bool TryGetByPlayerId(string playerId, out ManagedPlayer? managedPlayer)
        {
            return _playerIdMap.TryRead(playerId, out managedPlayer);
        }

        /// <summary>
        /// Gets all online players.
        /// </summary>
        /// <returns>An enumerable collection of online players.</returns>
        public static IEnumerable<ManagedPlayer> GetAll()
        {
            return _entityIdMap.GetValues();
        }

        /// <summary>
        /// Gets the number of online players.
        /// </summary>
        public static int Count => _entityIdMap.Count;
    }
}
