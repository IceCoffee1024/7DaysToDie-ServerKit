using IceCoffee.SimpleCRUD.OptionalAttributes;
using SdtdServerKit.Models;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// 积分变动日志实体
    /// </summary>
    public class T_PointLog
    {
        /// <summary>
        /// 日志 Id（自增）
        /// </summary>
        [PrimaryKey, IgnoreUpdate, IgnoreInsert]
        public int Id { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [IgnoreUpdate]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 玩家跨平台 Id
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家名（记录时刻的快照，可能为空）
        /// </summary>
        public string? PlayerName { get; set; }

        /// <summary>
        /// 日志分类
        /// </summary>
        public PointLogCategory Category { get; set; } = PointLogCategory.Other;

        /// <summary>
        /// 变化类型（增加/减少/设置）
        /// </summary>
        public PointChangeType ChangeType { get; set; } = PointChangeType.NoChange;

        /// <summary>
        /// 变动积分（正数=加，负数=减）
        /// </summary>
        public int Spend { get; set; }

        /// <summary>
        /// 操作后余额
        /// </summary>
        public int Balance { get; set; }

        /// <summary>
        /// 日志详情
        /// </summary>
        public string? Note { get; set; }
    }
}
