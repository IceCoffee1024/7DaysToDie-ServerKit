namespace SdtdServerKit.FunctionSettings
{
    /// <summary>
    /// Colored Chat Settings
    /// </summary>
    public class ColoredChatSettings : SettingsBase
    {
        /// <summary>
        /// Global Default Color
        /// </summary>
        public string? GlobalDefault { get; set; }

        /// <summary>
        /// Whisper Default Color
        /// </summary>
        public string? WhisperDefault { get; set; }

        /// <summary>
        /// Friends Default Color
        /// </summary>
        public string? FriendsDefault { get; set; }

        /// <summary>
        /// Party Default Color
        /// </summary>
        public string? PartyDefault { get; set; }
    }
}
