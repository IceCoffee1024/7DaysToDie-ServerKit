namespace SdtdServerKit
{
    /// <summary>
    /// 日志输出级别
    /// </summary>
    public enum CustomLogLevel
    {
        /// <summary>关闭所有日志输出</summary>
        None = 0,
        /// <summary>仅输出错误</summary>
        Error = 1,
        /// <summary>输出警告及以上</summary>
        Warn = 2,
        /// <summary>输出信息及以上（默认级别）</summary>
        Info = 3,
        /// <summary>输出调试及以上（最详细，会产生大量日志）</summary>
        Debug = 4
    }

    /// <summary>
    /// 自定义日志
    /// </summary>
    internal static class CustomLogger
    {
        public const string Prefix = "[天依] ";

        /// <summary>
        /// 当前日志级别，默认 Info
        /// </summary>
        public static CustomLogLevel CurrentLogLevel { get; private set; } = CustomLogLevel.Info;

        /// <summary>
        /// 是否将日志同步输出到 SdtdConsole（游戏控制台）。
        /// </summary>
        public static bool EnableConsoleOutput { get; private set; } = true;

        /// <summary>
        /// 配置日志器
        /// </summary>
        public static void Configure(string? logLevel, bool enableConsoleOutput)
        {
            if (!string.IsNullOrWhiteSpace(logLevel)
                && Enum.TryParse<CustomLogLevel>(logLevel!.Trim(), ignoreCase: true, out var level))
            {
                CurrentLogLevel = level;
            }

            EnableConsoleOutput = enableConsoleOutput;

            CustomLogger.Debug($"{Prefix}日志系统已初始化 - 级别: {CurrentLogLevel}, 控制台输出: {EnableConsoleOutput}");
        }

        /// <summary>
        /// 判断指定级别是否允许输出
        /// </summary>
        public static bool IsEnabled(CustomLogLevel level) => level <= CurrentLogLevel;

        /// <summary>
        /// 输出到 SdtdConsole
        /// </summary>
        private static void OutputToConsole(string message)
        {
            if (!EnableConsoleOutput) return;

            try
            {
                SdtdConsole.Instance.Output(message);
            }
            catch
            {
            }
        }

        private static void OutputToConsole(string message, params object[] args)
        {
            if (!EnableConsoleOutput) return;

            try
            {
                SdtdConsole.Instance.Output(message, args);
            }
            catch
            {
            }
        }

        #region Error

        public static void Error(string message)
        {
            if (!IsEnabled(CustomLogLevel.Error)) return;
            message = Prefix + message;
            Log.Error(message);
            OutputToConsole(message);
        }

        public static void Error(string message, params object[] args)
        {
            if (!IsEnabled(CustomLogLevel.Error)) return;
            message = Prefix + message;
            Log.Error(message, args);
            OutputToConsole(message, args);
        }

        public static void Error(Exception exception, string message)
        {
            if (!IsEnabled(CustomLogLevel.Error)) return;
            message = Prefix + message + Environment.NewLine + exception;
            Log.Error(message);
            OutputToConsole(message);
        }

        public static void Error(Exception exception, string message, params object[] args)
        {
            if (!IsEnabled(CustomLogLevel.Error)) return;
            message = Prefix + message + Environment.NewLine + exception;
            Log.Error(message, args);
            OutputToConsole(message, args);
        }

        #endregion

        #region Info

        public static void Info(string message)
        {
            if (!IsEnabled(CustomLogLevel.Info)) return;
            message = Prefix + message;
            Log.Out(message);
            OutputToConsole(message);
        }

        public static void Info(string message, params object[] args)
        {
            if (!IsEnabled(CustomLogLevel.Info)) return;
            message = Prefix + message;
            Log.Out(message, args);
            OutputToConsole(message, args);
        }

        public static void Info(Exception exception, string message)
        {
            if (!IsEnabled(CustomLogLevel.Info)) return;
            message = Prefix + message + Environment.NewLine + exception;
            Log.Out(message);
            OutputToConsole(message);
        }

        public static void Info(Exception exception, string message, params object[] args)
        {
            if (!IsEnabled(CustomLogLevel.Info)) return;
            message = Prefix + message + Environment.NewLine + exception;
            Log.Out(message, args);
            OutputToConsole(message, args);
        }

        #endregion

        #region Warn

        public static void Warn(string message)
        {
            if (!IsEnabled(CustomLogLevel.Warn)) return;
            message = Prefix + message;
            Log.Warning(message);
            OutputToConsole(message);
        }

        public static void Warn(string message, params object[] args)
        {
            if (!IsEnabled(CustomLogLevel.Warn)) return;
            message = Prefix + message;
            Log.Warning(message, args);
            OutputToConsole(message, args);
        }

        public static void Warn(Exception exception, string message)
        {
            if (!IsEnabled(CustomLogLevel.Warn)) return;
            message = Prefix + message + Environment.NewLine + exception;
            Log.Warning(message);
            OutputToConsole(message);
        }

        public static void Warn(Exception exception, string message, params object[] args)
        {
            if (!IsEnabled(CustomLogLevel.Warn)) return;
            message = Prefix + message + Environment.NewLine + exception;
            Log.Warning(message, args);
            OutputToConsole(message, args);
        }

        #endregion

        #region Debug

        /// <summary>
        /// 调试日志
        /// </summary>
        public static void Debug(string message)
        {
            if (!IsEnabled(CustomLogLevel.Debug)) return;
            message = Prefix + "[DEBUG] " + message;
            Log.Out(message);
            OutputToConsole(message);
        }

        public static void Debug(string message, params object[] args)
        {
            if (!IsEnabled(CustomLogLevel.Debug)) return;
            message = Prefix + "[DEBUG] " + message;
            Log.Out(message, args);
            OutputToConsole(message, args);
        }

        public static void Debug(Exception exception, string message)
        {
            if (!IsEnabled(CustomLogLevel.Debug)) return;
            message = Prefix + "[DEBUG] " + message + Environment.NewLine + exception;
            Log.Out(message);
            OutputToConsole(message);
        }

        #endregion
    }
}
