namespace SdtdServerKit.Extensions
{
    /// <summary>
    /// Extension methods for converting SpawnPosition to Model.
    /// </summary>
    public static class SpawnPositionExtension
    {
        /// <summary>
        /// Converts a SpawnPosition object to a Models.SpawnPosition object.
        /// </summary>
        /// <param name="spawnPosition">The SpawnPosition object to convert.</param>
        /// <returns>The converted Models.SpawnPosition object.</returns>
        public static Models.SpawnPosition ToModel(this SpawnPosition spawnPosition)
        {
            return new Models.SpawnPosition()
            {
                ClrIdx = spawnPosition.ClrIdx,
                Position = spawnPosition.position.ToPosition(),
                Heading = spawnPosition.heading,
            };
        }
    }
}
