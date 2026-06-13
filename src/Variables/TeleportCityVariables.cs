namespace SdtdServerKit.Variables
{
    public class TeleportCityVariables : VariablesBase
    {
        /// <summary>
        /// 城市Id
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        /// 城市名称
        /// </summary>
        public string CityName { get; set; }

        /// <summary>
        /// 传送间隔
        /// </summary>
        public int TeleInterval { get; set; }

        /// <summary>
        /// 需要积分
        /// </summary>
        public int PointsRequired { get; set; }

        /// <summary>
        /// 冷却时间, 单位: 秒
        /// </summary>
        public int CooldownSeconds { get; set; }
    }
}