namespace SdtdServerKit.Models
{
    /// <summary>
    /// Represents the undo history of an action.
    /// </summary>
    public class UndoHistory
    {
        /// <summary>
        /// Gets or sets the ID of the undo history.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the prefab associated with the undo history.
        /// </summary>
        public string PrefabName { get; set; }

        /// <summary>
        /// Gets or sets the position of the undo history.
        /// </summary>
        public Position Position { get; set; }

        /// <summary>
        /// Gets or sets the creation date and time of the undo history.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
