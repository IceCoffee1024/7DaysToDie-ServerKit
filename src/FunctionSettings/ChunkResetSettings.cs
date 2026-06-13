namespace SdtdServerKit.FunctionSettings
{
    /// <summary>
    /// 定时区域重置设置
    /// </summary>
    public class ChunkResetSettings : SettingsBase
    {
        /// <summary>
        /// 重置间隔（秒）
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// 重置前是否清除区域内的敌对实体
        /// </summary>
        public bool RemoveEnemiesBeforeReset { get; set; }

        /// <summary>
        /// 重置前公告提示（为空则不发送）
        /// </summary>
        public string ResetNoticeTip { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用区域提示Buff（玩家进入重置区域时添加Buff，离开时移除）
        /// </summary>
        public bool IsAreaBuffEnabled { get; set; }

        /// <summary>
        /// 区域提示Buff名称（游戏内已定义的Buff ID）
        /// </summary>
        public string AreaBuffName { get; set; } = "buffResetArea";

        /// <summary>
        /// 是否禁止在重置区域内放置领地石
        /// </summary>
        public bool IsLandClaimBanEnabled { get; set; }

        /// <summary>
        /// 是否禁止在重置区域内放置睡袋
        /// </summary>
        public bool IsBedrollBanEnabled { get; set; }

        /// <summary>
        /// 禁止放置领地石时的提示消息
        /// </summary>
        public string LandClaimBanTip { get; set; } = "[FF0000]禁止在定时重置区域内放置领地石！[-]";

        /// <summary>
        /// 禁止放置睡袋时的提示消息
        /// </summary>
        public string BedrollBanTip { get; set; } = "[FF0000]禁止在定时重置区域内放置睡袋！[-]";

        /// <summary>
        /// 是否禁止在系统房（POI 防护区域）内放置领地石
        /// </summary>
        public bool IsPoiLandClaimBanEnabled { get; set; }

        /// <summary>
        /// 是否禁止在系统房（POI 防护区域）内放置睡袋
        /// </summary>
        public bool IsPoiBedrollBanEnabled { get; set; }

        /// <summary>
        /// 系统房禁放领地石时的提示消息
        /// </summary>
        public string PoiLandClaimBanTip { get; set; } = "[FF0000]禁止在系统建筑区域内放置领地石！[-]";

        /// <summary>
        /// 系统房禁放睡袋时的提示消息
        /// </summary>
        public string PoiBedrollBanTip { get; set; } = "[FF0000]禁止在系统建筑区域内放置睡袋！[-]";
    }
}
