using System.Collections.Generic;
using UnityEngine;

namespace SdtdServerKit.MuteCommandAreas
{
    /// <summary>
    /// 区域命令禁用条目
    /// </summary>
    public class MuteCommandArea
    {
        /// <summary>
        /// 区域最小坐标
        /// </summary>
        public Vector2i PosA { get; set; }

        /// <summary>
        /// 区域最大坐标
        /// </summary>
        public Vector2i PosB { get; set; }

        /// <summary>
        /// 被禁用的命令列表
        /// </summary>
        public List<string> MutedCommands { get; set; } = new List<string>();
    }
}
