using IceCoffee.Common.Extensions;

namespace SdtdServerKit.Constants
{
    /// <summary>
    /// Locales
    /// </summary>
    public struct Locales
    {
        /// <summary>
        /// English
        /// </summary>
        public const string EN = "en";

        /// <summary>
        /// German
        /// </summary>
        public const string DE = "de";

        /// <summary>
        /// Spanish
        /// </summary>
        public const string ES = "es";

        /// <summary>
        /// French
        /// </summary>
        public const string FR = "fr";

        /// <summary>
        /// Italian
        /// </summary>
        public const string IT = "it";

        /// <summary>
        /// Japanese
        /// </summary>
        public const string JA = "ja";

        /// <summary>
        /// Korean
        /// </summary>
        public const string KO = "ko";

        /// <summary>
        /// Polish
        /// </summary>
        public const string PL = "pl";

        /// <summary>
        /// Portuguese
        /// </summary>
        public const string PT = "pt";

        /// <summary>
        /// Russian
        /// </summary>
        public const string RU = "ru";

        /// <summary>
        /// Turkish
        /// </summary>
        public const string TR = "tr";

        /// <summary>
        /// Simplified Chinese
        /// </summary>
        public const string ZH = "zh";

        /// <summary>
        /// Traditional Chinese
        /// </summary>
        public const string TW = "tw";

        /// <summary>
        /// Get locale by language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string Get(Language language)
        {
            switch (language)
            {
                case Language.English:
                    return EN;
                case Language.German:
                    return DE;
                case Language.Spanish:
                    return ES;
                case Language.French:
                    return FR;
                case Language.Italian:
                    return IT;
                case Language.Japanese:
                    return JA;
                case Language.Koreana:
                    return KO;
                case Language.Polish:
                    return PL;
                case Language.Brazilian:
                    return PT;
                case Language.Russian:
                    return RU;
                case Language.Turkish:
                    return TR;
                case Language.Schinese:
                    return ZH;
                case Language.Tchinese:
                    return TW;
                default:
                    return EN;
            }
        }

        /// <summary>
        /// Get locale by language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string Get(string language)
        {
            if(Enum.TryParse<Language>(language, true, out var lang))
            {
                return Get(lang);
            }

            return EN;
        }
    }
}
