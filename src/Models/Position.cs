namespace SdtdServerKit.Models
{
    /// <summary>
    /// Position
    /// </summary>
    public struct Position
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Position(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Position(float x, float y, float z)
        {
            X = (int)x;
            Y = (int)y;
            Z = (int)z;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Position(double x, double y, double z)
        {
            X = (int)x;
            Y = (int)y;
            Z = (int)z;
        }

        /// <summary>
        /// X
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Z
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string ToString()
        {
            return $"{X} {Y} {Z}";
        }
    }
}