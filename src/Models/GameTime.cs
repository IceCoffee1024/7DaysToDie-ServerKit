namespace SdtdServerKit.Models
{
    /// <summary>
    /// 游戏时间
    /// </summary>
    public class GameTime
    {
        /// <summary>
        /// 天
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        /// 小时
        /// </summary>
        public int Hours { get; set; }

        /// <summary>
        /// 分钟
        /// </summary>
        public int Minutes { get; set; }

        public GameTime()
        { 
        }

        public GameTime(int days, int hours, int minutes)
        {
            Days = days;
            Hours = hours;
            Minutes = minutes;
        }

        //public GameTime(ulong worldTime)
        //{
        //    Days = GameUtils.WorldTimeToDays(worldTime);
        //    Hours = GameUtils.WorldTimeToHours(worldTime);
        //    Minutes = GameUtils.WorldTimeToMinutes(worldTime);
        //}
    }
}