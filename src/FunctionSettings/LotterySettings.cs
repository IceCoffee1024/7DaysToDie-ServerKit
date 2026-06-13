namespace SdtdServerKit.FunctionSettings
{
    /// <summary>
    /// 抽奖设置
    /// </summary>
    public class LotterySettings : SettingsBase
    {
        /// <summary>
        /// 抽奖命令
        /// </summary>
        public string DrawCommand { get; set; } = "cj";

        /// <summary>
        /// 查询抽奖池列表命令
        /// </summary>
        public string QueryListCmd { get; set; } = "cjlist";

        /// <summary>
        /// 抽奖成功提示
        /// </summary>
        public string DrawSuccessTip { get; set; } = "[00FF00]恭喜你抽中了: {ItemName} x{Count}![-]";

        /// <summary>
        /// 积分不足提示
        /// </summary>
        public string PointsNotEnoughTip { get; set; } = "[FF0000]积分不足! 抽奖需要 {DrawCost} 积分, 你当前有 {Points} 积分[-]";

        /// <summary>
        /// 冷却中提示
        /// </summary>
        public string CoolingTip { get; set; } = "[FFFF00]抽奖冷却中! 请等待 {Seconds} 秒后再试[-]";

        /// <summary>
        /// 抽奖池不存在提示
        /// </summary>
        public string LotteryNotFoundTip { get; set; } = "[FF0000]抽奖池不存在![-]";

        /// <summary>
        /// 抽奖池未启用提示
        /// </summary>
        public string LotteryDisabledTip { get; set; } = "[FF0000]该抽奖池未启用![-]";

        /// <summary>
        /// 抽奖池为空提示
        /// </summary>
        public string LotteryEmptyTip { get; set; } = "[FF0000]抽奖池中没有物品![-]";

        /// <summary>
        /// 抽奖池列表提示
        /// </summary>
        public string LotteryItemTip { get; set; } = "[00FFFF]ID: {LotteryId} | 名称: {LotteryName} | 消耗: {DrawCost}积分 | 冷却: {DrawInterval}秒[-]";

        /// <summary>
        /// 没有抽奖池提示
        /// </summary>
        public string NoLottery { get; set; } = "[FFFF00]当前没有可用的抽奖池![-]";
    }
}
