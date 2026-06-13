using System;
using UnityEngine;

namespace SdtdServerKit.TraderProtectAreas
{
    /// <summary>
    /// 自定义商人保护区域条目
    /// </summary>
    public class TraderProtectArea
    {
        /// <summary>
        /// 唯一Id
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
        /// 区域备注名称（可选）
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 判断指定坐标是否在本区域内
        /// </summary>
        public bool Contains(int x, int z)
        {
            return x >= PosA.x && x <= PosB.x && z >= PosA.y && z <= PosB.y;
        }
    }
}
