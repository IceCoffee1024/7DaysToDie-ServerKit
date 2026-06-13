namespace SdtdServerKit.Models
{
    /// <summary>
    /// Represents a chat message.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Gets or sets the entity ID.
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the player ID.
        /// </summary>
        public string? PlayerId { get; set; }

        /// <summary>
        /// Gets or sets the sender's name.
        /// </summary>
        public required string SenderName { get; set; }

        /// <summary>
        /// Gets or sets the chat type.
        /// </summary>
        public ChatType ChatType { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public required string Message { get; set; }

        /// <summary>
        /// Gets or sets the creation time.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}