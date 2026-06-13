namespace SdtdServerKit.Models
{
    public class HomeLocation
    {
        /// <summary>
        /// 玩家Id
        /// </summary>
        public required string PlayerId { get; set; }

        /// <summary>
        /// 玩家名称
        /// </summary>
        public required string PlayerName { get; set; }

        /// <summary>
        /// Home名称
        /// </summary>
        public required string HomeName { get; set; }

        /// <summary>
        /// 三维坐标
        /// </summary>
        public required string Position { get; set; }
    }
}
