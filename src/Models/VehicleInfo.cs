namespace SdtdServerKit.Models
{
    /// <summary>
    /// 载具信息
    /// </summary>
    public class VehicleInfo : EntityInfo
    {
        /// <summary>
        /// 载具所有者ID
        /// </summary>
        public string OwnerId { get; set; }

        /// <summary>
        /// 载具所有者名称
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// 是否已锁定
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// 载具实体类型（如 EntityVJeep）
        /// </summary>
        public string VehicleEntityClass { get; set; }

        /// <summary>
        /// 载具本地化名称（如 "自行车" 或 "Bicycle"）
        /// </summary>
        public string LocalizedName { get; set; }
    }
}
