namespace SdtdServerKit.WebSockets
{
    internal class WebSocketMessage
    {
        [JsonProperty("modEventType")]
        public ModEventType ModEventType { get; set; }

        public WebSocketMessage(ModEventType modEventType)
        {
            ModEventType = modEventType;
        }
    }

    internal class WebSocketMessage<TData>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("modEventType")]
        public ModEventType ModEventType { get; set; }

        [JsonProperty("data")]
        public TData? Data { get; set; }

        public WebSocketMessage(ModEventType modEventType, TData data)
        {
            ModEventType = modEventType;
            Data = data;
        }
    }
}
