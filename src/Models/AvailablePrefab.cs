namespace SdtdServerKit.Models
{
    /// <summary>
    /// Represents a loaded prefab.
    /// </summary>
    public class AvailablePrefab
    {
        /// <summary>
        /// Gets or sets the name of the prefab.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the localization name of the prefab.
        /// </summary>
        public string? LocalizationName { get; set; }

        /// <summary>
        /// Gets or sets the full path of the prefab.
        /// </summary>
        public string? FullPath { get; set; }

        ///// <summary>
        ///// Gets or sets the size of the prefab.
        ///// </summary>
        //public string Size { get; set; }

        ///// <summary>
        ///// Gets or sets the tags of the prefab.
        ///// </summary>
        //public string Tags { get; set; }

        ///// <summary>
        ///// Gets or sets the theme tags of the prefab.
        ///// </summary>
        //public string ThemeTags { get; set; }

        ///// <summary>
        ///// Gets or sets the Y offset of the prefab.
        ///// </summary>
        //public int YOffset { get; set; }
    }
}
