namespace SdtdServerKit.Models
{
    /// <summary>
    /// 区域命令禁用信息
    /// </summary>
    public class MuteCommandAreaInfo
    {
        /// <summary>
        /// 区域最小 X
        /// </summary>
        public int MinX { get; set; }

        /// <summary>
        /// 区域最小 Z
        /// </summary>
        public int MinZ { get; set; }

        /// <summary>
        /// 区域最大 X
        /// </summary>
        public int MaxX { get; set; }

        /// <summary>
        /// 区域最大 Z
        /// </summary>
        public int MaxZ { get; set; }

        /// <summary>
        /// 被禁用的命令列表
        /// </summary>
        public List<string> MutedCommands { get; set; } = new List<string>();
    }

    /// <summary>
    /// 区域命令禁用请求
    /// </summary>
    public class MuteCommandRequest
    {
        /// <summary>
        /// 区域坐标 X1
        /// </summary>
        public int X1 { get; set; }
        /// <summary>
        /// 区域坐标 Z1
        /// </summary>
        public int Z1 { get; set; }
        /// <summary>
        /// 区域坐标 X2
        /// </summary>
        public int X2 { get; set; }
        /// <summary>
        /// 区域坐标 Z2
        /// </summary>
        public int Z2 { get; set; }
        /// <summary>
        /// 命令名称
        /// </summary>
        public string Command { get; set; }
    }

    /// <summary>
    /// 更新区域命令禁用请求
    /// </summary>
    public class UpdateMuteCommandRequest
    {
        /// <summary>
        /// 旧区域坐标 X1
        /// </summary>
        public int OldX1 { get; set; }
        /// <summary>
        /// 旧区域坐标 Z1
        /// </summary>
        public int OldZ1 { get; set; }
        /// <summary>
        /// 旧区域坐标 X2
        /// </summary>
        public int OldX2 { get; set; }
        /// <summary>
        /// 旧区域坐标 Z2
        /// </summary>
        public int OldZ2 { get; set; }
        /// <summary>
        /// 新区域坐标 X1
        /// </summary>
        public int NewX1 { get; set; }
        /// <summary>
        /// 新区域坐标 Z1
        /// </summary>
        public int NewZ1 { get; set; }
        /// <summary>
        /// 新区域坐标 X2
        /// </summary>
        public int NewX2 { get; set; }
        /// <summary>
        /// 新区域坐标 Z2
        /// </summary>
        public int NewZ2 { get; set; }
        /// <summary>
        /// 新命令名称（多个命令用逗号分隔）
        /// </summary>
        public string NewCommand { get; set; }
    }
}
