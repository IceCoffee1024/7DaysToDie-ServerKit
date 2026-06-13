using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// 等级礼包领取记录（用于通用礼包）
    /// </summary>
    [Table("T_LevelGiftClaimRecord_v1")]
    public class T_LevelGiftClaimRecord
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        [PrimaryKey]
        public string PlayerId { get; set; }

        /// <summary>
        /// 礼包ID
        /// </summary>
        [PrimaryKey]
        public string GiftId { get; set; }

        /// <summary>
        /// 领取时间
        /// </summary>
        public DateTime ClaimAt { get; set; }
    }
}
