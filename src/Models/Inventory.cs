namespace SdtdServerKit.Models
{
    /// <summary>
    /// 库存
    /// </summary>
    public class Inventory
    {
        /// <summary>
        /// 背包
        /// </summary>
        public IEnumerable<InvItem> Bag { get; set; }

        /// <summary>
        /// 腰带
        /// </summary>
        public IEnumerable<InvItem> Belt { get; set; }

        /// <summary>
        /// 装备
        /// </summary>
        public IEnumerable<InvItem> Equipment { get; set; }
    }
}
