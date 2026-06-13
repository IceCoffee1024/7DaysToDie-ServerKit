namespace SdtdServerKit.FunctionSettings
{
    public class GameNoticeSettings : SettingsBase
    {
        /// <summary>
        /// 欢迎通知
        /// </summary>
        public string WelcomeNotice { get; set; }

        /// <summary>
        /// 轮播通知
        /// </summary>
        public string[] RotatingNotices { get; set; }

        /// <summary>
        /// 轮播间隔
        /// </summary>
        public int RotatingInterval { get; set; }

        /// <summary>
        /// 血月通知1, 下一个血月在 {BloodMoonDays} 天后
        /// </summary>
        public string BloodMoonNotice1 { get; set; }

        /// <summary>
        /// 血月通知2, 下一个血月就在今天。将于 {BloodMoonStartTime} 开始
        /// </summary>
        public string BloodMoonNotice2 { get; set; }

        /// <summary>
        /// 血月通知3, 血月来了, 坚持到 {BloodMoonEndTime}
        /// </summary>
        public string BloodMoonNotice3 { get; set; }
    }
}