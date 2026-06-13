namespace SdtdServerKit.Models
{
    /// <summary>
    /// 物品方块种类
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ItemBlockKind
    {
        /// <summary>
        /// 全部
        /// </summary>
        All,

        /// <summary>
        /// 物品
        /// </summary>
        Item,

        /// <summary>
        /// 方块
        /// </summary>
        Block,
    }
}