namespace SdtdServerKit.Models
{
    /// <summary>
    /// 积分日志分类
    /// </summary>
    public enum PointLogCategory
    {
        /// <summary>商店购买</summary>
        Shop = 1,

        /// <summary>签到积分</summary>
        SignIn = 2,

        /// <summary>积分转账</summary>
        Transfer = 3,

        /// <summary>传送相关</summary>
        Teleport = 4,

        /// <summary>击杀奖励 / 死亡惩罚</summary>
        ZombieKill = 5,

        /// <summary>抽奖</summary>
        Lottery = 6,

        /// <summary>兑换码（CDKey）</summary>
        CdKey = 7,

        /// <summary>等级礼包</summary>
        LevelGift = 8,

        /// <summary>VIP 礼包</summary>
        VipGift = 9,

        /// <summary>Web 面板操作</summary>
        WebApi = 10,

        /// <summary>外部 Mod / 第三方组件操作</summary>
        External = 12,

        /// <summary>其它</summary>
        Other = 99,
    }

    /// <summary>
    /// 积分变化类型
    /// </summary>
    public enum PointChangeType
    {
        /// <summary>无变化</summary>
        NoChange = 0,

        /// <summary>增加</summary>
        Add = 1,

        /// <summary>减少</summary>
        Sub = 2,

        /// <summary>直接设置</summary>
        Set = 3,
    }
}
