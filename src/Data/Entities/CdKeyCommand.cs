using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// Represents a CdKeyCommand entity.
    /// </summary>
    [Table("CdKeyCommand")]
    public class CdKeyCommand
    {
        /// <summary>
        /// Cd Key Id
        /// </summary>
        [Column("[CdKeyId]")]
        [PrimaryKey]
        public int CdKeyId { get; set; }
		
        /// <summary>
        /// Command Id
        /// </summary>
        [Column("[CommandId]")]
        [PrimaryKey]
        public int CommandId { get; set; }
		
    }
}