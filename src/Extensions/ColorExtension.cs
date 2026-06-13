using UnityEngine;

namespace SdtdServerKit.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="Color"/> class.
    /// </summary>
    public static class ColorExtension
    {
        /// <summary>
        /// Converts a Color object to its hexadecimal representation.
        /// </summary>
        /// <param name="color">The Color object to convert.</param>
        /// <returns>The hexadecimal representation of the Color object.</returns>
        public static string ToHex(this Color color)
        {
            return string.Format("{0:X02}{1:X02}{2:X02}", (int)(color.r * 255), (int)(color.g * 255), (int)(color.b * 255));
        }
    }
}
