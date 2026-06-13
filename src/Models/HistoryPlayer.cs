using SdtdServerKit.Managers;

namespace SdtdServerKit.Models
{
    /// <summary>
    /// Represents a history playerfor response model.
    /// </summary>
    public class HistoryPlayer : PlayerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryPlayer"/> class.
        /// </summary>
        public HistoryPlayer(PersistentPlayerData persistentPlayerData)
            : base(persistentPlayerData.PrimaryId.CombinedString,
                  persistentPlayerData.PlayerName.DisplayName,
                  persistentPlayerData.EntityId,
                  persistentPlayerData.NativeId.CombinedString)
        {
            string playerId = persistentPlayerData.PrimaryId.CombinedString;
            PlayerDataFile playerDataFile;
            if (LivePlayerManager.TryGetByPlayerId(playerId, out var managedPlayer))
            {
                playerDataFile = managedPlayer!.PlayerDataFile;
            }
            else
            {
                playerDataFile = new PlayerDataFile();
                playerDataFile.Load(GameIO.GetPlayerDataDir(), playerId);
                IsOffline = true;
            }

            PlayerDetails = new PlayerDetails(playerDataFile, persistentPlayerData);
        }

        /// <summary>
        /// Gets a value indicating whether the player is offline.
        /// </summary>
        public bool IsOffline { get; set; }

        /// <summary>
        /// Gets the player details.
        /// </summary>
        public PlayerDetails PlayerDetails { get; set; }
    }
}
