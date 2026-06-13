namespace SdtdServerKit.Models
{
    /// <summary>
    /// Represents the skill of a player.
    /// </summary>
    public class PlayerSkill
    {
        /// <summary>
        /// Gets or sets the name of the skill.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the localized name of the skill.
        /// </summary>
        public string? LocalizationName { get; set; }

        /// <summary>
        /// Gets or sets the localized description of the skill.
        /// </summary>
        public string? LocalizationDesc { get; set; }

        ///// <summary>
        ///// Gets or sets the localized long description of the skill.
        ///// </summary>
        //public string? LocalizationLongDesc { get; set; }

        /// <summary>
        /// Gets or sets the level of the skill.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the minimum level of the skill.
        /// </summary>
        public int MinLevel { get; set; }

        /// <summary>
        /// Gets or sets the maximum level of the skill.
        /// </summary>
        public int MaxLevel { get; set; }

        /// <summary>
        /// Gets or sets the cost for the next level of the skill.
        /// </summary>
        public int CostForNextLevel { get; set; }

        /// <summary>
        /// Gets or sets the icon of the skill.
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Gets or sets the type of the skill.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the children progressions of the skill.
        /// </summary>
        public List<PlayerSkill>? Children { get; set; }
    }
}
