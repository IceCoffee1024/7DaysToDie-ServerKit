using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// 自定义商人保护区域实体
    /// </summary>
    public class T_TraderProtectArea
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
        /// 区域最小X坐标
        /// </summary>
        public int MinX { get; set; }

        /// <summary>
        /// 区域最小Z坐标
        /// </summary>
        public int MinZ { get; set; }

        /// <summary>
        /// 区域最大X坐标
        /// </summary>
        public int MaxX { get; set; }

        /// <summary>
        /// 区域最大Z坐标
        /// </summary>
        public int MaxZ { get; set; }

        /// <summary>
        /// 区域备注名称（可选）
        /// </summary>
        public string? Name { get; set; }
    }
}
