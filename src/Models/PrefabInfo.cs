namespace SdtdServerKit.Models
{
    /// <summary>
    /// POI 预制件信息
    /// </summary>
    public class PrefabInfo
    {
        /// <summary>
        /// 预制件 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 预制件名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// X 坐标
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y 坐标
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Z 坐标
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        /// X 方向大小
        /// </summary>
        public int SizeX { get; set; }

        /// <summary>
        /// Y 方向大小
        /// </summary>
        public int SizeY { get; set; }

        /// <summary>
        /// Z 方向大小
        /// </summary>
        public int SizeZ { get; set; }

        /// <summary>
        /// 是否为商人区
        /// </summary>
        public bool IsTrader { get; set; }

        /// <summary>
        /// 旋转
        /// </summary>
        public byte Rotation { get; set; }
    }
}
