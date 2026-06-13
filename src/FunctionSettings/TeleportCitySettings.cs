namespace SdtdServerKit.FunctionSettings
{
    public class TeleportCitySettings : SettingsBase
    {
        /// <summary>
        /// 查询列表命令
        /// </summary>
        public string QueryListCmd { get; set; }

        /// <summary>
        /// 传送命令前缀
        /// </summary>
        public string TeleCmdPrefix { get; set; }

        /// <summary>
        /// 传送间隔, 单位: 秒
        /// </summary>
        public int TeleInterval { get; set; }

        /// <summary>
        /// 查询列表提示
        /// </summary>
        public string LocationItemTip { get; set; }

        /// <summary>
        /// 传送成功提示
        /// </summary>
        public string TeleSuccessTip { get; set; }

        /// <summary>
        /// 积分不足提示
        /// </summary>
        public string PointsNotEnoughTip { get; set; }

        /// <summary>
        /// 正在冷却提示
        /// </summary>
        public string CoolingTip { get; set; }

        /// <summary>
        /// 无城市信息提示
        /// </summary>
        public string NoLocation { get; set; }
    }
}