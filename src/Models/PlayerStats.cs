namespace SdtdServerKit.Models
{
    /// <summary>
    /// Represents the statistics of a player.
    /// </summary>
    public class PlayerStats
    {
        /// <summary>
        /// Gets or sets the health of the player.
        /// </summary>
        public float Health { get; set; }

        /// <summary>
        /// Gets or sets the stamina of the player.
        /// </summary>
        public float Stamina { get; set; }

        /// <summary>
        /// Gets or sets the core temperature of the player.
        /// </summary>
        public float CoreTemp { get; set; }

        /// <summary>
        /// Gets or sets the food level of the player.
        /// </summary>
        public float Food { get; set; }

        /// <summary>
        /// Gets or sets the water level of the player.
        /// </summary>
        public float Water { get; set; }
    }
}
