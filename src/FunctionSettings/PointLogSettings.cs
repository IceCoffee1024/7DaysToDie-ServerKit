namespace SdtdServerKit.FunctionSettings
{
    /// <summary>
    /// 积分日志设置
    /// </summary>
    public class PointLogSettings : SettingsBase
    {
        /// <summary>商店购买</summary>
        public bool LogShop { get; set; } = true;

        /// <summary>签到积分</summary>
        public bool LogSignIn { get; set; } = true;

        /// <summary>积分转账</summary>
        public bool LogTransfer { get; set; } = true;

        /// <summary>传送相关</summary>
        public bool LogTeleport { get; set; } = true;

        /// <summary>击杀奖励 / 死亡惩罚</summary>
        public bool LogZombieKill { get; set; }

        /// <summary>抽奖</summary>
        public bool LogLottery { get; set; } = true;

        /// <summary>兑换码（CDKey）</summary>
        public bool LogCdKey { get; set; } = true;

        /// <summary>等级礼包</summary>
        public bool LogLevelGift { get; set; } = true;

        /// <summary>VIP 礼包</summary>
        public bool LogVipGift { get; set; } = true;

        /// <summary>Web 面板操作</summary>
        public bool LogWebApi { get; set; } = true;

        /// <summary>外部 Mod / 第三方组件操作</summary>
        public bool LogExternal { get; set; } = true;

        /// <summary>其它（保底分类）</summary>
        public bool LogOther { get; set; } = true;

        /// <summary>
        /// 自动清理：保留天数（&gt;0 时启用，每小时检查一次，0 表示永不清理）
        /// </summary>
        public int RetentionDays { get; set; }
    }
}
