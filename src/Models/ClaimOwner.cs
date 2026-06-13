namespace SdtdServerKit.Models
{
    /// <summary>
    /// 领地石所有者
    /// </summary>
    public class ClaimOwner : PlayerBase
    {
        /// <summary>
        /// Claim Active
        /// </summary>
        public bool ClaimActive { get; set; }

        /// <summary>
        /// Last Login
        /// </summary>
        public DateTime LastLogin { get; set; }

        /// <summary>
        /// Claim Positions
        /// </summary>
        public IEnumerable<Position> ClaimPositions { get; set; }
    }
}