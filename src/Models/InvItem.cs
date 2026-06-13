namespace SdtdServerKit.Models
{
    /// <summary>
    /// 库存项目
    /// </summary>
    public class InvItem
    {
        /// <summary>
        /// 项目名称
        /// </summary>
        public required string ItemName { get; set; }

        /// <summary>
        /// 本地化名称
        /// </summary>
        public required string LocalizationName { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 最大堆叠允许
        /// </summary>
        public int MaxStackAllowed { get; set; }

        /// <summary>
        /// 质量
        /// </summary>
        public int Quality { get; set; }

        /// <summary>
        /// 质量颜色
        /// </summary>
        public string? QualityColor { get; set; }

        /// <summary>
        /// 已使用次数
        /// </summary>
        public float UseTimes { get; set; }

        /// <summary>
        /// 最大使用次数
        /// </summary>
        public int MaxUseTimes { get; set; }

        /// <summary>
        /// 是否是模组
        /// </summary>
        public bool IsMod { get; set; }

        /// <summary>
        /// 随机种子
        /// </summary>
        public ushort Seed { get; set; }

        /// <summary>
        /// 部件
        /// </summary>
        public InvItem?[]? Parts { get; set; }
    }
}
