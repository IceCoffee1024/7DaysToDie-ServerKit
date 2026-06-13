using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// PVP/PVE 混合区域实体
    /// </summary>
    public class T_PvpVeArea
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
        /// 杀戮模式: 0=无伤害, 1=队友伤害, 2=陌生人伤害, 3=所有人伤害
        /// </summary>
        public int KillMode { get; set; } = 2;

        /// <summary>
        /// 死亡掉包模式: 0=不掉包, 1=全部掉落, 2=只掉腰带, 3=只掉背包
        /// </summary>
        public int DropOnDeath { get; set; }

        /// <summary>
        /// 在线领地石硬度加成（0为无敌）
        /// </summary>
        public int LandClaimOnline { get; set; } = 4;

        /// <summary>
        /// 离线领地石硬度加成（0为无敌）
        /// </summary>
        public int LandClaimOffline { get; set; } = 8;

        /// <summary>
        /// 区域提示Buff名称
        /// </summary>
        public string BuffName { get; set; } = string.Empty;

        /// <summary>
        /// 区域备注名称（可选）
        /// </summary>
        public string? Name { get; set; }
    }
}
