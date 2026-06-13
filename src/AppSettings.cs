namespace SdtdServerKit
{
    /// <summary>
    /// AppSettings
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string? Password { get; set; }
        /// <summary>
        /// 服务器地址
        /// </summary>
        public required string WebUrl { get; set; }
        /// <summary>
        /// WebSocket端口
        /// </summary>
        public int WebSocketPort { get; set; }
        /// <summary>
        /// WebSocket地址
        /// </summary>
        public required string WebSocketUrl { get; set; }
        /// <summary>
        /// AccessToken到期时间
        /// </summary>
        public int AccessTokenExpireTime { get; set; }
        /// <summary>
        /// 数据库路径
        /// </summary>
        public required string DatabasePath { get; set; }
        /// <summary>
        /// 服务器配置文件名
        /// </summary>
        public required string ServerSettingsFileName { get; set; }

        /// <summary>
        /// 日志级别：None/Error/Warn/Info/Debug，默认 Info
        /// - None：关闭所有日志
        /// - Error：仅输出错误
        /// - Warn：输出警告及以上
        /// - Info：输出常规信息
        /// - Debug：输出详细调试信息
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string LogLevel { get; set; } = "Info";

        /// <summary>
        /// 是否将日志同步输出到游戏控制台
        /// </summary>
        public bool EnableConsoleOutput { get; set; } = true;
    }
}