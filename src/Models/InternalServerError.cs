namespace SdtdServerKit.Models
{
    /// <summary>
    /// Internal Server Error
    /// </summary>
    public class InternalServerError
    {
        [JsonProperty("message")]
        public required string Message { get; set; }

        [JsonProperty("exceptionMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string? ExceptionMessage { get; set; }

        [JsonProperty("exceptionType", NullValueHandling = NullValueHandling.Ignore)]
        public string? ExceptionType { get; set; }

        [JsonProperty("StackTrace", NullValueHandling = NullValueHandling.Ignore)]
        public string? StackTrace { get; set; }
    }
}
