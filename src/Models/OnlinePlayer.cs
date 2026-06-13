namespace SdtdServerKit.Models
{
    /// <summary>
    /// Represents an online player for response model.
    /// </summary>
    public class OnlinePlayer : PlayerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnlinePlayer"/> class.
        /// </summary>
        public OnlinePlayer(IManagedPlayer managedPlayer) : base(managedPlayer)
        {
            var clientInfo = managedPlayer.ClientInfo!;
            var entityPlayer = managedPlayer.EntityPlayer!;
            Ip = clientInfo.ip;
            Ping = clientInfo.ping;
            GameStage = entityPlayer.gameStage;
            PlayerDetails = new PlayerDetails(managedPlayer.PlayerDataFile, managedPlayer.PersistentPlayerData, entityPlayer);
        }

        /// <summary>
        /// 使用实体内存中的数据。
        /// </summary>
        public OnlinePlayer(ClientInfo clientInfo, EntityPlayer entityPlayer)
            : base(
                clientInfo.InternalId?.CombinedString ?? string.Empty,
                clientInfo.playerName ?? entityPlayer.EntityName ?? string.Empty,
                clientInfo.entityId,
                clientInfo.PlatformId?.CombinedString ?? string.Empty)
        {
            Ip = clientInfo.ip ?? string.Empty;
            Ping = clientInfo.ping;
            GameStage = entityPlayer.gameStage;

            var pos = entityPlayer.GetPosition();
            var details = new PlayerDetails
            {
                Position = pos.ToPosition(),
                LastSpawnPosition = entityPlayer.lastSpawnPosition.ToModel(),
                LastLogin = DateTime.Now,
                PlayerKills = entityPlayer.KilledPlayers,
                ZombieKills = entityPlayer.KilledZombies,
                Deaths = entityPlayer.Died,
                Score = entityPlayer.Score,
                Level = entityPlayer.Progression?.Level ?? 1,
                ExpToNextLevel = entityPlayer.Progression?.ExpToNextLevel ?? 0,
                SkillPoints = entityPlayer.Progression?.SkillPoints ?? 0,
                CurrentLife = 0f,
                LongestLife = 0f,
                TotalTimePlayed = 0f,
            };

            var stats = entityPlayer.Stats;
            if (stats != null)
            {
                details.Stats = new PlayerStats
                {
                    Health = stats.Health.Value,
                    Stamina = stats.Stamina.Value,
                    Food = stats.Food.Value,
                    Water = stats.Water.Value,
                };
            }

            PlayerDetails = details;
        }

        /// <summary>
        /// Gets the IP address.
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Gets the ping value.
        /// </summary>
        public int Ping { get; set; }

        /// <summary>
        /// Gets the game stage of the player.
        /// </summary>
        public int GameStage { get; set; }

        /// <summary>
        /// Gets the player details.
        /// </summary>
        public PlayerDetails PlayerDetails { get; set; }
    }
}
