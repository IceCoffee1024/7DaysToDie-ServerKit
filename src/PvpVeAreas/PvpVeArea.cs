using System;
using UnityEngine;

namespace SdtdServerKit.PvpVeAreas
{
    /// <summary>
    /// PVP/PVE 混合区域条目（运行时模型）
    /// </summary>
    public class PvpVeArea
    {
        /// <summary>
        /// 唯一Id（默认区域为 0）
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 区域最小坐标（x = MinX, y = MinZ）
        /// </summary>
        public Vector2i PosA { get; set; }

        /// <summary>
        /// 区域最大坐标（x = MaxX, y = MaxZ）
        /// </summary>
        public Vector2i PosB { get; set; }

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

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 判断指定坐标是否在本区域内（仅 X/Z 平面）
        /// </summary>
        public bool Contains(int x, int z)
        {
            return x >= PosA.x && x <= PosB.x && z >= PosA.y && z <= PosB.y;
        }

        /// <summary>
        /// 判断本区域内的某个核心规则是否与目标区域相同
        /// </summary>
        public bool RulesEqual(PvpVeArea other)
        {
            if (other == null) return false;
            return KillMode == other.KillMode
                && DropOnDeath == other.DropOnDeath
                && LandClaimOnline == other.LandClaimOnline
                && LandClaimOffline == other.LandClaimOffline
                && string.Equals(BuffName ?? string.Empty, other.BuffName ?? string.Empty, StringComparison.Ordinal);
        }
    }
}
