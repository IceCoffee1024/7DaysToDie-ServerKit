using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// 商品
    /// </summary>
    [Table("T_Goods_v2")]
    public class T_Goods
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
        /// 名称
        /// </summary>
        public required string Name { get; set; }
        
        /// <summary>
        /// 价格
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string? Description { get; set; }
    }
}