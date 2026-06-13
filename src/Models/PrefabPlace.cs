namespace SdtdServerKit.Models
{
    /// <summary>
    /// Represents a prefab placement.
    /// </summary>
    public class PrefabPlace
    {
        /// <summary>
        /// Gets or sets the name of the prefab file.
        /// </summary>
        public required string PrefabFileName { get; set; }

        /// <summary>
        /// Gets or sets the position of the prefab.
        /// </summary>
        public Position Position { get; set; }

        /// <summary>
        /// Gets or sets the rotation of the prefab.
        /// </summary>
        public Rotation Rotation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sleepers are disabled in the prefab.
        /// </summary>
        public bool NoSleepers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the prefab should be added to the random world generation.
        /// </summary>
        public bool AddToRWG { get; set; }
    }
}
