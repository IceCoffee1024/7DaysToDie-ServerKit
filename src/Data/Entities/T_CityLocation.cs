using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// 城市位置
    /// </summary>
    [Table("T_CityLocation_v1")]
    public class T_CityLocation
    {
        /// <summary>
        /// 唯一Id
        /// </summary>
        [PrimaryKey]
        public int Id { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [IgnoreUpdate]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 城市名称
        /// </summary>
        public required string CityName { get; set; }

        /// <summary>
        /// 传送需要积分
        /// </summary>
        public int PointsRequired { get; set; }

        /// <summary>
        /// 三维坐标
        /// </summary>
        public required string Position { get; set; }

        /// <summary>
        /// 视角方向
        /// </summary>
        public string? ViewDirection { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 单独的传送命令
        /// </summary>
        public string? TeleCommand { get; set; }
    }
}