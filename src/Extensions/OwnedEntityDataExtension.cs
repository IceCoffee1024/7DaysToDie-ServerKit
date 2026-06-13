namespace SdtdServerKit.Extensions
{
    /// <summary>
    /// Extension methods for converting OwnedEntityData to OwnedEntity.
    /// </summary>
    public static class OwnedEntityDataExtension
    {
        /// <summary>
        /// Converts an instance of OwnedEntityData to an instance of OwnedEntity.
        /// </summary>
        /// <param name="entity">The OwnedEntityData instance to convert.</param>
        /// <returns>The converted OwnedEntity instance.</returns>
        public static OwnedEntity ToModel(this OwnedEntityData entity)
        {
            return new OwnedEntity()
            {
                Id = entity.Id,
                ClassId = entity.ClassId,
                LastKnownPosition = entity.LastKnownPosition.ToPosition(),
                HasLastKnownPosition = entity.hasLastKnownPosition
            };
        }

        /// <summary>
        /// Converts a collection of OwnedEntityData instances to a collection of OwnedEntity instances.
        /// </summary>
        /// <param name="entities">The collection of OwnedEntityData instances to convert.</param>
        /// <returns>The converted collection of OwnedEntity instances.</returns>
        public static IEnumerable<OwnedEntity> ToModels(this IEnumerable<OwnedEntityData> entities)
        {
            return entities.Select(i => i.ToModel());
        }
    }
}
