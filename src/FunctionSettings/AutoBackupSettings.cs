namespace SdtdServerKit.FunctionSettings
{
    /// <summary>
    /// 自动备份设置
    /// </summary>
    public class AutoBackupSettings : SettingsBase
    {
        /// <summary>
        /// 自动备份间隔
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// 保留文件数量限制
        /// </summary>
        public int RetainedFileCountLimit { get; set; }

        /// <summary>
        /// 是否在手动备份后重置计时
        /// </summary>
        public bool ResetIntervalAfterManualBackup { get; set; }

        /// <summary>
        /// 是否在没有玩家时跳过备份
        /// </summary>
        public bool SkipIfThereAreNoPlayers { get; set; }

        /// <summary>
        /// 是否在服务器启动时自动备份
        /// </summary>
        public bool AutoBackupOnServerStartup { get; set; }

        /// <summary>
        /// 备份文件夹
        /// </summary>
        public required string ArchiveFolder { get; set; }
    }
}