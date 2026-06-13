namespace SdtdServerKit.Variables
{
    public class PointsSystemVariables : VariablesBase
    {
        /// <summary>
        /// 签到奖励积分
        /// </summary>
        public int SignInRewardPoints { get; set; }

        /// <summary>
        /// 玩家总积分
        /// </summary>
        public int PlayerTotalPoints { get; set; }

        /// <summary>
        /// 游戏币数量
        /// </summary>
        public int CurrencyAmount { get; set; }
    }
}