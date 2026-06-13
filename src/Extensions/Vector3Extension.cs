using UnityEngine;

namespace SdtdServerKit.Extensions
{
    internal static class Vector3Extension
    {
        /// <summary>
        /// Converts a Vector3 to a Position.
        /// </summary>
        /// <param name="v">The Vector3 to convert.</param>
        /// <returns>The converted Position.</returns>
        public static Position ToPosition(this Vector3 v)
        {
            return new Position(v.x, v.y, v.z);
        }

        /// <summary>
        /// Converts a Vector3i to a Position.
        /// </summary>
        /// <param name="v">The Vector3i to convert.</param>
        /// <returns>The converted Position.</returns>
        public static Position ToPosition(this Vector3i v)
        {
            return new Position(v.x, v.y, v.z);
        }

        /// <summary>
        /// Converts a collection of Vector3 to a collection of Position.
        /// </summary>
        /// <param name="v">The collection of Vector3 to convert.</param>
        /// <returns>The converted collection of Position.</returns>
        public static IEnumerable<Position> ToPositions(this IEnumerable<Vector3> v)
        {
            return v.Select(i => i.ToPosition());
        }

        /// <summary>
        /// Converts a collection of Vector3i to a collection of Position.
        /// </summary>
        /// <param name="v">The collection of Vector3i to convert.</param>
        /// <returns>The converted collection of Position.</returns>
        public static IEnumerable<Position> ToPositions(this IEnumerable<Vector3i> v)
        {
            return v.Select(i => i.ToPosition());
        }
    }
}