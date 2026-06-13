namespace SdtdServerKit.Models
{
    /// <summary>
    /// OAuth Result
    /// </summary>
    public class OAuthResult
    {
        /// <summary>
        /// Access Token
        /// </summary>
        [JsonProperty("access_token")]
        public required string AccessToken { get; set; }

        /// <summary>
        /// Token Type
        /// </summary>
        [JsonProperty("token_type")]
        public required string TokenType { get; set; }

        /// <summary>
        /// Expires In
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}