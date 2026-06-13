namespace SdtdServerKit.FunctionSettings
{
    /// <summary>
    /// 单个僵尸类型的击杀奖励配置
    /// </summary>
    public class ZombieKillRewardEntry
    {
        /// <summary>
        /// 僵尸实体类名
        /// </summary>
        public string EntityClassName { get; set; }

        /// <summary>
        /// 击杀奖励积分（0 表示不奖励积分）
        /// </summary>
        public int RewardPoints { get; set; }

        /// <summary>
        /// 击杀后执行的指令列表
        /// </summary>
        public string[] ExecuteCommands { get; set; } = Array.Empty<string>();
    }

    public class Trigger
    {
        /// <summary>
        /// Gets or sets a value indicating whether the trigger is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否启用击杀通知
        /// </summary>
        public bool IsEnableKillNotification { get; set; }

        /// <summary>
        /// Gets or sets the commands to execute when the trigger is activated.
        /// </summary>
        public string[] ExecuteCommands { get; set; }

        /// <summary>
        /// 按丧尸类型配置的击杀奖励
        /// </summary>
        public ZombieKillRewardEntry[] ZombieRewards { get; set; } = Array.Empty<ZombieKillRewardEntry>();

        /// <summary>
        /// 死亡惩罚扣除积分
        /// </summary>
        public int DeathPenaltyPoints { get; set; }

        /// <summary>
        /// 是否启用死亡惩罚通知
        /// </summary>
        public bool IsEnableDeathNotification { get; set; }
    }
    public class AutoRestart
    {
        /// <summary>
        /// Gets or sets a value indicating whether auto restart is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the hour at which the server should restart.
        /// </summary>
        public int RestartHour { get; set; }

        /// <summary>
        /// Gets or sets the minute at which the server should restart.
        /// </summary>
        public int RestartMinute { get; set; }

        /// <summary>
        /// Gets or sets the messages to display before the server restarts.
        /// </summary>
        public string[] Messages { get; set; }
    }

    /// <summary>
    /// 出生点配置
    /// </summary>
    public class SpawnPointSettings
    {
        /// <summary>
        /// 是否启用出生点传送
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 出生点坐标列表
        /// </summary>
        public SpawnPointEntry[] SpawnPoints { get; set; } = Array.Empty<SpawnPointEntry>();
    }

    /// <summary>
    /// 单个出生点配置
    /// </summary>
    public class SpawnPointEntry
    {
        /// <summary>
        /// 出生点坐标（格式：x,y,z）
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// 出生点描述
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Global settings.
    /// </summary>
    public class GlobalSettings : SettingsBase
    {
        /// <summary>
        /// Gets or sets the global server name.
        /// </summary>
        public string GlobalServerName { get; set; }

        /// <summary>
        /// Gets or sets the whisper server name.
        /// </summary>
        public string WhisperServerName { get; set; }

        /// <summary>
        /// Gets or sets the chat command prefix.
        /// </summary>
        public string ChatCommandPrefix { get; set; }

        /// <summary>
        /// Gets or sets the chat command separator.
        /// </summary>
        public string ChatCommandSeparator { get; set; }

        /// <summary>
        /// Gets or sets the error message for handling chat messages.
        /// </summary>
        public string HandleChatMessageError { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to check for zombies around the player before teleporting.
        /// </summary>
        public bool TeleZombieCheck { get; set; }

        /// <summary>
        /// Gets or sets the disable teleportation tip.
        /// </summary>
        public string TeleDisableTip { get; set; }

        /// <summary>
        /// Gets or sets the trigger for killing zombies.
        /// </summary>
        public Trigger KillZombieTrigger { get; set; }

        /// <summary>
        /// Gets or sets the trigger for player death.
        /// </summary>
        public Trigger DeathTrigger { get; set; }

        /// <summary>
        /// Gets or sets the auto restart settings.
        /// </summary>
        public AutoRestart AutoRestart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to block family sharing accounts from joining the server.
        /// </summary>
        public bool BlockFamilySharingAccount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool EnableFallingBlockProtection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable automatic zombie cleanup.
        /// </summary>
        public bool EnableAutoZombieCleanup { get; set; }

        /// <summary>
        /// Gets or sets the auto zombie cleanup threshold.
        /// </summary>
        public int AutoZombieCleanupThreshold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable XMLs secondary overwrites.
        /// </summary>
        public bool EnableXmlsSecondaryOverwrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide the command in chat.
        /// </summary>
        public bool HideCommandInChat { get; set; }

        /// <summary>
        /// 获取或设置生成点设置.
        /// </summary>
        public SpawnPointSettings SpawnPointSettings { get; set; }
    }
}