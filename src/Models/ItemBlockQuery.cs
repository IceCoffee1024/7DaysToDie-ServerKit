using System.ComponentModel;

namespace SdtdServerKit.Models
{
    /// <summary>
    /// 物品方块查询参数
    /// </summary>
    public class ItemBlockQuery
    {
        /// <summary>
        /// 物品方块种类
        /// </summary>
        [DefaultValue(ItemBlockKind.All)]
        public ItemBlockKind ItemBlockKind { get; set; } = ItemBlockKind.All;

        /// <summary>
        /// 页码
        /// </summary>
        [DefaultValue(1)]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 每页数量, 值小于 0 时返回所有记录
        /// </summary>
        [DefaultValue(10)]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 语言
        /// </summary>
        [DefaultValue(Language.English)]
        public Language Language { get; set; } = Language.English;

        /// <summary>
        /// 搜索关键词
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 是否展示开发物品方块
        /// </summary>
        [DefaultValue(false)]
        public bool ShowUserHidden { get; set; }
    }
}