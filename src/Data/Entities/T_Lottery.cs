using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// 抽奖配置
    /// </summary>
    [Table("T_Lottery_v1")]
    public class T_Lottery
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
        /// 抽奖池名称
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 抽奖命令
        /// </summary>
        public string? DrawCommand { get; set; }

        /// <summary>
        /// 抽奖间隔（秒）
        /// </summary>
        public int DrawInterval { get; set; }

        /// <summary>
        /// 单次抽奖消耗积分
        /// </summary>
        public int DrawCost { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string? Description { get; set; }
    }
}
