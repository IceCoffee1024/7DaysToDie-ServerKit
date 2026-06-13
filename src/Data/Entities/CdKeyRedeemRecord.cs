using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// Represents a CdKeyRedeemRecord entity.
    /// </summary>
    [Table("CdKeyRedeemRecord")]
    public class CdKeyRedeemRecord
    {
        /// <summary>
        /// Id
        /// </summary>
        [Column("[Id]")]
        [PrimaryKey]
        [IgnoreInsert, IgnoreUpdate]
        public int Id { get; set; }
		
        /// <summary>
        /// Created At
        /// </summary>
        [Column("[CreatedAt]")]
        [IgnoreUpdate]
        public DateTime CreatedAt { get; set; }
		
        /// <summary>
        /// Key
        /// </summary>
        [Column("[Key]")]
        public required string Key { get; set; }
		
        /// <summary>
        /// Player Id
        /// </summary>
        [Column("[PlayerId]")]
        public required string PlayerId { get; set; }
		
        /// <summary>
        /// Player Name
        /// </summary>
        [Column("[PlayerName]")]
        public required string PlayerName { get; set; }
		
    }
}