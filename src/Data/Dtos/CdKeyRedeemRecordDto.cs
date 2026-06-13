
namespace SdtdServerKit.Data.Dtos
{
    /// <summary>
    /// Represents a CdKeyRedeemRecord dto.
    /// </summary>
    public class CdKeyRedeemRecordDto
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
        /// Player Id
        /// </summary>
        public required string PlayerId { get; set; }
        
        /// <summary>
        /// Player Name
        /// </summary>
        public required string PlayerName { get; set; }
        
    }
    
    /// <summary>
    /// Represents a CdKeyRedeemRecord create dto.
    /// </summary>
    public class CdKeyRedeemRecordCreateDto
    {
        /// <summary>
        /// Key
        /// </summary>
        public required string Key { get; set; }
        
        /// <summary>
        /// Player Id
        /// </summary>
        public required string PlayerId { get; set; }
        
        /// <summary>
        /// Player Name
        /// </summary>
        public required string PlayerName { get; set; }
        
    }
    
    /// <summary>
    /// Represents a CdKeyRedeemRecord update dto.
    /// </summary>
    public class CdKeyRedeemRecordUpdateDto
    {
        /// <summary>
        /// Key
        /// </summary>
        public required string Key { get; set; }
        
        /// <summary>
        /// Player Id
        /// </summary>
        public required string PlayerId { get; set; }
        
        /// <summary>
        /// Player Name
        /// </summary>
        public required string PlayerName { get; set; }
        
    }
}