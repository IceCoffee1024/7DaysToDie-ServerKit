using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// Represents a CdKeyItem entity.
    /// </summary>
    [Table("CdKeyItem")]
    public class CdKeyItem
    {
        /// <summary>
        /// Cd Key Id
        /// </summary>
        [Column("[CdKeyId]")]
        [PrimaryKey]
        public int CdKeyId { get; set; }
		
        /// <summary>
        /// Item Id
        /// </summary>
        [Column("[ItemId]")]
        [PrimaryKey]
        public int ItemId { get; set; }
		
    }
}