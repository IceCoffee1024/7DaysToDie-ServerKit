namespace SdtdServerKit.FunctionSettings
{
    public class GameStoreSettings : SettingsBase
    {
        /// <summary>
        /// 查询商品列表命令
        /// </summary>
        public string QueryListCmd { get; set; }

        /// <summary>
        /// 购买命令前缀
        /// </summary>
        public string BuyCmdPrefix { get; set; }

        /// <summary>
        /// 商品项目提示
        /// </summary>
        public string GoodsItemTip { get; set; }

        /// <summary>
        /// 购买成功提示
        /// </summary>
        public string BuySuccessTip { get; set; }

        /// <summary>
        /// 积分不足提示
        /// </summary>
        public string PointsNotEnoughTip { get; set; }

        /// <summary>
        /// 没有商品提示
        /// </summary>
        public string NoGoods { get; set; }
    }
}