namespace SdtdServerKit.FunctionSettings
{
    /// <summary>
    /// 自定义商人保护区域设置
    /// </summary>
    public class TraderProtectSettings : SettingsBase
    {
        /// <summary>
        /// 是否启用区域提示Buff（玩家进入保护区时添加Buff，离开时移除）
        /// </summary>
        public bool IsAreaBuffEnabled { get; set; }

        /// <summary>
        /// 区域提示Buff名称（游戏内已定义的Buff ID）
        /// </summary>
        public string AreaBuffName { get; set; } = "buffTraderProtect";
    }
}
