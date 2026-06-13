namespace SdtdServerKit.Models
{
    /// <summary>
    /// Represents the details of a player.
    /// </summary>
    public class PlayerDetails
    {
        /// <summary>
        /// 初始化。
        /// </summary>
        public PlayerDetails()
        {
            Stats = new PlayerStats();
            SpawnPoints = Array.Empty<Position>();
            AlreadyCraftedList = Array.Empty<string>();
            UnlockedRecipeList = Array.Empty<string>();
            FavoriteRecipeList = Array.Empty<string>();
            OwnedEntities = Array.Empty<OwnedEntity>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerDetails"/> class.
        /// </summary>
        public PlayerDetails(PlayerDataFile playerDataFile, PersistentPlayerData persistentPlayerData, EntityPlayer? entityPlayer = null)
        {
            IsAdmin = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(persistentPlayerData.PrimaryId) == 0;
            Position = playerDataFile.ecd.pos.ToPosition();
            LastSpawnPosition = playerDataFile.lastSpawnPosition.ToModel();
            LastLogin = persistentPlayerData.LastLogin;
            PlayerKills = playerDataFile.playerKills;
            ZombieKills = playerDataFile.zombieKills;
            Deaths = playerDataFile.deaths;
            Score = playerDataFile.score;

            var stats = playerDataFile.ecd.stats;
            if(stats != null)
            {
                Stats = new PlayerStats()
                {
                    Health = stats.Health.Value,
                    Stamina = stats.Stamina.Value,
                    //CoreTemp = stats.CoreTemp.Value,
                    Food = stats.Food.Value,
                    Water = stats.Water.Value
                };
            }
            else
            {
                Stats = new PlayerStats();
            }

            if (entityPlayer != null)
            {
                Level = entityPlayer.Progression.Level;
                ExpToNextLevel = entityPlayer.Progression.ExpToNextLevel;
                SkillPoints = entityPlayer.Progression.SkillPoints;
            }
            else
            {
                var stream = playerDataFile.progressionData;
                if (stream.Length > 0L)
                {
                    using var binaryReader = MemoryPools.poolBinaryReader.AllocSync(false);
                    stream.Position = 0L;
                    binaryReader.SetBaseStream(stream);

                    byte b = binaryReader.ReadByte();
                    Level = binaryReader.ReadUInt16();
                    ExpToNextLevel = binaryReader.ReadInt32();
                    SkillPoints = binaryReader.ReadUInt16();
                }
            }

            LandProtectionActive = GameManager.Instance.World.IsLandProtectionValidForPlayer(persistentPlayerData);
            DistanceWalked = playerDataFile.distanceWalked;
            TotalItemsCrafted = playerDataFile.totalItemsCrafted;
            LongestLife = playerDataFile.longestLife;
            CurrentLife = playerDataFile.currentLife;
            TotalTimePlayed = playerDataFile.totalTimePlayed;
            RentedVMPosition = playerDataFile.rentedVMPosition.ToPosition();
            RentalEndTime = playerDataFile.rentalEndTime;
            RentalEndDay = playerDataFile.rentalEndDay;
            SpawnPoints = playerDataFile.spawnPoints.ToPositions();
            AlreadyCraftedList = playerDataFile.alreadyCraftedList;
            UnlockedRecipeList = playerDataFile.unlockedRecipeList;
            FavoriteRecipeList = playerDataFile.favoriteRecipeList;
            OwnedEntities = playerDataFile.ownedEntities.ToModels();
        }

        /// <summary>
        /// Gets the player's admin status.
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Gets the position of the player.
        /// </summary>
        public Position Position { get; set; }

        /// <summary>
        /// Gets the last spawn position of the player.
        /// </summary>
        public SpawnPosition LastSpawnPosition { get; set; }

        /// <summary>
        /// Gets the last login time of the player.
        /// </summary>
        public DateTime LastLogin { get; set; }

        /// <summary>
        /// Gets the number of kills by the player.
        /// </summary>
        public int PlayerKills { get; set; }

        /// <summary>
        /// Gets the number of zombie kills by the player.
        /// </summary>
        public int ZombieKills { get; set; }

        /// <summary>
        /// Gets the number of deaths by the player.
        /// </summary>
        public int Deaths { get; set; }

        /// <summary>
        /// Gets the score of the player.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Gets the player stats.
        /// </summary>
        public PlayerStats Stats { get; set; }

        /// <summary>
        /// Gets or sets the level of the player.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the experience required to reach the next level.
        /// </summary>
        public int ExpToNextLevel { get; set; }

        /// <summary>
        /// Gets or sets the skill points available for the player.
        /// </summary>
        public int SkillPoints { get; set; }

        /// <summary>
        /// Gets a value indicating whether land protection is active for the player.
        /// </summary>
        public bool LandProtectionActive { get; set; }

        /// <summary>
        /// Gets the distance walked by the player.
        /// </summary>
        public float DistanceWalked { get; set; }

        /// <summary>
        /// Gets the total number of items crafted by the player.
        /// </summary>
        public uint TotalItemsCrafted { get; set; }

        /// <summary>
        /// Gets the longest life of the player.
        /// </summary>
        public float LongestLife { get; set; }

        /// <summary>
        /// Gets the current life of the player.
        /// </summary>
        public float CurrentLife { get; set; }

        /// <summary>
        /// Gets the total time played by the player in minutes.
        /// </summary>
        public float TotalTimePlayed { get; set; }

        /// <summary>
        /// Gets the rented VM position of the player.
        /// </summary>
        public Position RentedVMPosition { get; set; }

        /// <summary>
        /// Gets the rental end time of the player.
        /// </summary>
        public ulong RentalEndTime { get; set; }

        /// <summary>
        /// Gets the rental end day of the player.
        /// </summary>
        public int RentalEndDay { get; set; }

        /// <summary>
        /// Gets the spawn points of the player.
        /// </summary>
        public IEnumerable<Position> SpawnPoints { get; set; }

        /// <summary>
        /// Gets the list of already crafted items by the player.
        /// </summary>
        public IEnumerable<string> AlreadyCraftedList { get; set; }

        /// <summary>
        /// Gets the list of unlocked recipes by the player.
        /// </summary>
        public IEnumerable<string> UnlockedRecipeList { get; set; }

        /// <summary>
        /// Gets the list of favorite recipes by the player.
        /// </summary>
        public IEnumerable<string> FavoriteRecipeList { get; set; }

        /// <summary>
        /// Gets the list of owned entities by the player.
        /// </summary>
        public IEnumerable<OwnedEntity> OwnedEntities { get; set; } 

        /// <summary>
        /// Gets or sets the count of points.
        /// </summary>
        public int PointsCount { get; set; }
    }
}