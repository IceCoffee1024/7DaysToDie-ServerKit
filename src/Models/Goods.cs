namespace SdtdServerKit.Models
{
    /// <summary>
    /// 商品
    /// </summary>
    public class Goods
    {
        /// <summary>
        /// 商品Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 商品名称
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
