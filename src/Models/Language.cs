namespace SdtdServerKit.Models
{
    /// <summary>
    /// Language
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Language
    {
        ///// <summary>
        ///// Key
        ///// </summary>
        //Key = -1,

        /// <summary>
        /// File
        /// </summary>
        File = 0,

        /// <summary>
        /// Type
        /// </summary>
        Type,

        /// <summary>
        /// Used in Main Menu
        /// </summary>
        UsedInMainMenu,

        /// <summary>
        /// No Translate
        /// </summary>
        NoTranslate,

        /// <summary>
        /// English
        /// </summary>
        English,

        /// <summary>
        /// Context Alternate Text
        /// </summary>
        ContextAlternateText,

        /// <summary>
        /// German
        /// </summary>
        German,

        /// <summary>
        /// Spanish
        /// </summary>
        Spanish,

        /// <summary>
        /// French
        /// </summary>
        French,

        /// <summary>
        /// Italian
        /// </summary>
        Italian,

        /// <summary>
        /// Japanese
        /// </summary>
        Japanese,

        /// <summary>
        /// Koreana
        /// </summary>
        Koreana,

        /// <summary>
        /// Polish
        /// </summary>
        Polish,

        /// <summary>
        /// Brazilian
        /// </summary>
        Brazilian,

        /// <summary>
        /// Russian
        /// </summary>
        Russian,

        /// <summary>
        /// Turkish
        /// </summary>
        Turkish,

        /// <summary>
        /// Simplified Chinese
        /// </summary>
        Schinese,

        /// <summary>
        /// Traditional Chinese
        /// </summary>
        Tchinese
    }
}
