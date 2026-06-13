namespace SdtdServerKit.Models
{
    /// <summary>
    /// 生成类型
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RespawnType
    {
        /// <summary>
        /// 新游戏
        /// </summary>
        NewGame = 0,

        /// <summary>
        /// 已加载游戏
        /// </summary>
        LoadedGame = 1,

        /// <summary>
        /// 死亡
        /// </summary>
        Died = 2,

        /// <summary>
        /// 传送
        /// </summary>
        Teleport = 3,

        /// <summary>
        /// 新玩家进入多人游戏
        /// </summary>
        EnterMultiplayer = 4,

        /// <summary>
        /// 旧玩家加入多人游戏
        /// </summary>
        JoinMultiplayer = 5,

        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 6
    }
}