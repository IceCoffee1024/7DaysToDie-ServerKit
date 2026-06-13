using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// Represents a CdKey entity.
    /// </summary>
    [Table("CdKey")]
    public class CdKey
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
        /// Redeem Count
        /// </summary>
        [Column("[RedeemCount]")]
        public int RedeemCount { get; set; }
		
        /// <summary>
        /// Max Redeem Count
        /// </summary>
        [Column("[MaxRedeemCount]")]
        public int MaxRedeemCount { get; set; }
		
        /// <summary>
        /// Expiry At
        /// </summary>
        [Column("[ExpiryAt]")]
        public DateTime? ExpiryAt { get; set; }
		
        /// <summary>
        /// Description
        /// </summary>
        [Column("[Description]")]
        public string? Description { get; set; }
		
    }
}