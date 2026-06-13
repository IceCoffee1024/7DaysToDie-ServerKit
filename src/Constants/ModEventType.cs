namespace SdtdServerKit.Constants
{
    /// <summary>
    /// ModEventType
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ModEventType
    {
        /// <summary>
        /// 欢迎
        /// </summary>
        Welcome,

        /// <summary>
        /// 命令执行回复
        /// </summary>
        CommandExecutionReply,

        /// <summary>
        /// 日志回调
        /// </summary>
        LogCallback,

        /// <summary>
        /// 游戏唤醒
        /// </summary>
        GameAwake,

        /// <summary>
        /// 游戏启动完成
        /// </summary>
        GameStartDone,

        /// <summary>
        /// 游戏更新
        /// </summary>
        GameUpdate,

        /// <summary>
        /// 游戏关闭
        /// </summary>
        GameShutdown,

        /// <summary>
        /// 计算区块颜色完成
        /// </summary>
        CalcChunkColorsDone,

        /// <summary>
        /// 聊天消息
        /// </summary>
        ChatMessage,

        /// <summary>
        /// 实体被杀
        /// </summary>
        EntityKilled,

        /// <summary>
        /// 实体生成
        /// </summary>
        EntitySpawned,

        /// <summary>
        /// 玩家断开连接
        /// </summary>
        PlayerDisconnected,

        /// <summary>
        /// 玩家登录
        /// </summary>
        PlayerLogin,

        /// <summary>
        /// 玩家生成在世界中
        /// </summary>
        PlayerSpawnedInWorld,

        /// <summary>
        /// 玩家生成
        /// </summary>
        PlayerSpawning,

        /// <summary>
        /// 保存玩家数据
        /// </summary>
        SavePlayerData,

        /// <summary>
        /// 
        /// </summary>
        SkyChanged,
    }
}
