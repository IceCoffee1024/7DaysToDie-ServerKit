namespace SdtdServerKit.Models
{
    /// <summary>
    /// 领地石
    /// </summary>
    public class LandClaims
    {
        /// <summary>
        /// 领地石拥有者们
        /// </summary>
        public IEnumerable<ClaimOwner> ClaimOwners { get; set; }

        /// <summary>
        /// 领地石范围
        /// </summary>
        public int ClaimSize { get; set; }
    }
}