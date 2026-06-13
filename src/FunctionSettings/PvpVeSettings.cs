namespace SdtdServerKit.FunctionSettings
{
    /// <summary>
    /// PVP/PVE 混合区域设置
    /// </summary>
    public class PvpVeSettings : SettingsBase
    {
        /// <summary>
        /// 默认区域杀戮模式：0=无伤害, 1=队友伤害, 2=陌生人伤害, 3=所有人伤害
        /// </summary>
        public int DefaultKillMode { get; set; } = 2;

        /// <summary>
        /// 默认区域死亡掉包模式：0=不掉包, 1=全部掉落, 2=只掉腰带, 3=只掉背包
        /// </summary>
        public int DefaultDropOnDeath { get; set; }

        /// <summary>
        /// 默认区域在线领地石硬度加成（0表示无敌）
        /// </summary>
        public int DefaultLandClaimOnline { get; set; } = 4;

        /// <summary>
        /// 默认区域离线领地石硬度加成（0表示无敌）
        /// </summary>
        public int DefaultLandClaimOffline { get; set; } = 8;

        /// <summary>
        /// 默认区域提示Buff（玩家在自定义区域之外（默认区域）时，给予的提示Buff）
        /// </summary>
        public string DefaultBuffName { get; set; } = "buffPvpVeNoticePve";
    }
}
