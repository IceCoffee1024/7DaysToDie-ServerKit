using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// 传送记录
    /// </summary>
    public class T_TeleRecord
    {
        /// <summary>
        /// 唯一Id
        /// </summary>
        [PrimaryKey, IgnoreUpdate, IgnoreInsert]
        public int Id { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [IgnoreUpdate]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 玩家Id
        /// </summary>
        public required string PlayerId { get; set; }

        /// <summary>
        /// 玩家名称
        /// </summary>
        public required string PlayerName { get; set; }

        /// <summary>
        /// 目标类型
        /// </summary>
        public required string TargetType { get; set; }

        /// <summary>
        /// 目标名称
        /// </summary>
        public required string TargetName { get; set; }

        /// <summary>
        /// 源坐标, 空格分割
        /// </summary>
        public required string OriginPosition { get; set; }

        /// <summary>
        /// 目的坐标, 空格分割
        /// </summary>
        public required string TargetPosition { get; set; }
    }
}