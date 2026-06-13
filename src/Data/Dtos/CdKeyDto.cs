
namespace SdtdServerKit.Data.Dtos
{
    /// <summary>
    /// Represents a CdKey dto.
    /// </summary>
    public class CdKeyDto
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Created At
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Key
        /// </summary>
        public required string Key { get; set; }
        
        /// <summary>
        /// Redeem Count
        /// </summary>
        public int RedeemCount { get; set; }
        
        /// <summary>
        /// Max Redeem Count
        /// </summary>
        public int MaxRedeemCount { get; set; }
        
        /// <summary>
        /// Expiry At
        /// </summary>
        public DateTime? ExpiryAt { get; set; }
        
        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }
        
    }
    
    /// <summary>
    /// Represents a CdKey create dto.
    /// </summary>
    public class CdKeyCreateDto
    {
        /// <summary>
        /// Key
        /// </summary>
        public required string Key { get; set; }
        
        /// <summary>
        /// Redeem Count
        /// </summary>
        public int RedeemCount { get; set; }
        
        /// <summary>
        /// Max Redeem Count
        /// </summary>
        public int MaxRedeemCount { get; set; }
        
        /// <summary>
        /// Expiry At
        /// </summary>
        public DateTime? ExpiryAt { get; set; }
        
        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }
        
    }
    
    /// <summary>
    /// Represents a CdKey update dto.
    /// </summary>
    public class CdKeyUpdateDto
    {
        /// <summary>
        /// Key
        /// </summary>
        public required string Key { get; set; }
        
        /// <summary>
        /// Redeem Count
        /// </summary>
        public int RedeemCount { get; set; }
        
        /// <summary>
        /// Max Redeem Count
        /// </summary>
        public int MaxRedeemCount { get; set; }
        
        /// <summary>
        /// Expiry At
        /// </summary>
        public DateTime? ExpiryAt { get; set; }
        
        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }
        
    }
}