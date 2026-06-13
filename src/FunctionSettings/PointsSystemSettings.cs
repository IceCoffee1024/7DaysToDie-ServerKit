namespace SdtdServerKit.FunctionSettings
{
    public class PointsSystemSettings : SettingsBase
    {
        /// <summary>
        /// 签到命令
        /// </summary>
        public string SignInCmd { get; set; }

        /// <summary>
        /// 签到间隔秒数
        /// </summary>
        public int SignInInterval { get; set; }

        /// <summary>
        /// 签到奖励积分
        /// </summary>
        public int SignInRewardPoints { get; set; }

        /// <summary>
        /// 签到成功提示
        /// </summary>
        public string SignInSuccessTip { get; set; }

        /// <summary>
        /// 签到失败提示
        /// </summary>
        public string SignInFailureTip { get; set; }

        /// <summary>
        /// 查询积分命令
        /// </summary>
        public string QueryPointsCmd { get; set; }

        /// <summary>
        /// 查询积分提示
        /// </summary>
        public string QueryPointsTip { get; set; }

        /// <summary>
        /// 是否启用游戏内货币兑换积分
        /// </summary>
        public bool IsCurrencyExchangeEnabled { get; set; }

        /// <summary>
        /// 游戏内货币与积分兑换比例
        /// </summary>
        public double CurrencyToPointsExchangeRate { get; set; }

        /// <summary>
        /// 兑换命令
        /// </summary>
        public string CurrencyExchangeCmd { get; set; }

        /// <summary>
        /// 兑换成功提示
        /// </summary>
        public string ExchangeSuccessTip { get; set; }

        /// <summary>
        /// 兑换失败提示
        /// </summary>
        public string ExchangeFailureTip { get; set; }
    }
}
