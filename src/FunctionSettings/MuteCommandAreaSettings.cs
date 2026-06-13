namespace SdtdServerKit.FunctionSettings
{
    /// <summary>
    /// 区域命令禁用功能配置
    /// </summary>
    public class MuteCommandAreaSettings : SettingsBase
    {
        /// <summary>
        /// 命令被禁用时的提示消息
        /// 可用变量: 
        /// - {command} - 命令名称
        /// - {playerName} - 玩家名称
        /// - {x} - 玩家X坐标
        /// - {z} - 玩家Z坐标
        /// </summary>
        public string MutedCommandTip { get; set; } = "[FF0000]此区域禁止使用命令: {command}[-]";
    }
}
