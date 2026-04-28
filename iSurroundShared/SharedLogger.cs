using System;
using System.IO;
using NLog;

namespace iSurroundShared
{
    /// <summary>
    /// 统一日志管理器，提供集中式日志记录和级别控制
    /// </summary>
    /// <remarks>
    /// 日志级别（从低到高）：Trace → Debug → Info → Warn → Error → Fatal
    /// 
    /// 使用示例：
    /// 
    /// // 1. 初始化（通常在程序启动时调用一次）
    ///     SharedLogger.Initialize(myLogger);
    /// 
    /// // 2. 设置日志级别（代码级控制，不给用户UI）
    ///     SharedLogger.MinLevel = NLog.LogLevel.Warn;  // 只输出Warn及以上
    /// 
    /// // 或使用预设常量
    ///     SharedLogger.MinLevel = SharedLogger.LogLevel.Release;  // 发布版本
    ///     SharedLogger.MinLevel = SharedLogger.LogLevel.Debug;  // 调试版本
    /// 
    /// // 3. 使用日志
    ///     SharedLogger.Trace("trace信息");      // 最低级别
    ///     SharedLogger.Debug("debug信息");
    ///     SharedLogger.Info("info信息");
    ///     SharedLogger.Warn("warn信息");
    ///     SharedLogger.Error("error信息");
    ///     SharedLogger.Fatal("fatal信息");
    ///     SharedLogger.Error(ex, "异常信息");    // 记录异常
    /// 
    /// 预设级别快捷常量：
    ///     SharedLogger.LogLevel.Off     - 全部关闭
    ///     SharedLogger.LogLevel.Fatal  - 只Fatal（最严格）
    ///     SharedLogger.LogLevel.Error  - Error + Fatal（默认，发布用）
    ///     SharedLogger.LogLevel.Warn  - Warn + Error + Fatal
    ///     SharedLogger.LogLevel.Info  - Info及以上
    ///     SharedLogger.LogLevel.Debug - Debug及以上
    ///     SharedLogger.LogLevel.Trace - 全部输出（调试用）
    /// </remarks>
    public class SharedLogger
    {

        /// <summary>
        /// 预设日志级别常量，方便代码使用
        /// </summary>
        public static class LogLevel
        {
            public static readonly NLog.LogLevel Off = NLog.LogLevel.Off;
            public static readonly NLog.LogLevel Fatal = NLog.LogLevel.Fatal;
            public static readonly NLog.LogLevel Error = NLog.LogLevel.Error;
            public static readonly NLog.LogLevel Warn = NLog.LogLevel.Warn;
            public static readonly NLog.LogLevel Info = NLog.LogLevel.Info;
            public static readonly NLog.LogLevel Debug = NLog.LogLevel.Debug;
            public static readonly NLog.LogLevel Trace = NLog.LogLevel.Trace;
            
            /// <summary>
            /// 发布版本级别 - 只输出Error和Fatal，减少日志噪音
            /// </summary>
            public static readonly NLog.LogLevel Release = NLog.LogLevel.Error;
        }
        
        internal static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iSurround");
        internal static string AppLogPath = Path.Combine(AppDataPath, $"Logs");
        internal static string AppSettingsFile = Path.Combine(AppDataPath, $"Settings_1.0.json");
        
        private static NLog.Logger _logger;
        
        /// <summary>
        /// 最低日志级别，低于此级别的日志不会被输出
        /// 默认值：Error（只输出Error和Fatal）
        /// </summary>
        private static NLog.LogLevel _minLevel = NLog.LogLevel.Error;
        
        /// <summary>
        /// 获取或设置最低日志级别
        /// </summary>
        /// <example>
        /// // 发布给用户时设置为Warn（只记录警告和错误）
        /// SharedLogger.MinLevel = NLog.LogLevel.Warn;
        /// 
        /// // 调试时设置为Trace（全部输出）
        /// SharedLogger.MinLevel = NLog.LogLevel.Trace;
        /// 
        /// // 使用预设常量
        /// SharedLogger.MinLevel = SharedLogger.LogLevel.Release;
        /// SharedLogger.MinLevel = SharedLogger.LogLevel.Debug;
        /// 
        /// // 关闭所有日志
        /// SharedLogger.MinLevel = NLog.LogLevel.Off;
        /// </example>
        public static NLog.LogLevel MinLevel
        {
            get => _minLevel;
            set => _minLevel = value;
        }

        public static bool IsTraceEnabled => _logger?.IsTraceEnabled ?? false;
        public static bool IsDebugEnabled => _logger?.IsDebugEnabled ?? false;
        public static bool IsInfoEnabled => _logger?.IsInfoEnabled ?? false;
        public static bool IsWarnEnabled => _logger?.IsWarnEnabled ?? false;
        public static bool IsErrorEnabled => _logger?.IsErrorEnabled ?? false;
        public static bool IsFatalEnabled => _logger?.IsFatalEnabled ?? false;

        /// <summary>
        /// 获取或设置日志开关（向后兼容）
        /// true: 启用日志（按MinLevel过滤）
        /// false: 禁用所有日志（等同于MinLevel=Off）
        /// </summary>
        public static bool IsEnabled
        {
            get => _minLevel != NLog.LogLevel.Off;
            set => _minLevel = value ? NLog.LogLevel.Error : NLog.LogLevel.Off;
        }

        /// <summary>
        /// 获取NLog Logger实例，用于需要直接访问NLog的场景
        /// </summary>
        public static NLog.Logger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = NLog.LogManager.GetCurrentClassLogger();
                }
                return _logger;
            }
            private set => _logger = value;
        }

        /// <summary>
        /// 向后兼容属性，等同于Logger
        /// </summary>
        public static NLog.Logger logger => Logger;

        /// <summary>
        /// 初始化日志系统
        /// </summary>
        /// <param name="parentLogger">外部传入的NLog Logger实例，实现统一日志输出</param>
        public static void Initialize(NLog.Logger parentLogger)
        {
            _logger = parentLogger;
        }

        /// <summary>
        /// 初始化日志系统并设置最低级别
        /// </summary>
        /// <param name="parentLogger">外部传入的NLog Logger实例</param>
        /// <param name="minLevel">最低日志级别</param>
        public static void Initialize(NLog.Logger parentLogger, NLog.LogLevel minLevel)
        {
            _logger = parentLogger;
            _minLevel = minLevel;
        }

        /// <summary>
        /// 判断指定级别是否应该被记录
        /// </summary>
        private static bool ShouldLog(NLog.LogLevel level)
        {
            return level >= _minLevel;
        }

        /// <summary>
        /// 记录Trace级别日志（最低级别，用于详细调试信息）
        /// </summary>
        public static void Trace(string message)
        {
            if (ShouldLog(NLog.LogLevel.Trace)) Logger.Trace(message);
        }

        public static void Trace(string message, params object[] args)
        {
            if (ShouldLog(NLog.LogLevel.Trace)) Logger.Trace(message, args);
        }

        public static void Trace(Exception exception, string message)
        {
            if (ShouldLog(NLog.LogLevel.Trace)) Logger.Trace(exception, message);
        }

        /// <summary>
        /// 记录Debug级别日志（调试信息）
        /// </summary>
        public static void Debug(string message)
        {
            if (ShouldLog(NLog.LogLevel.Debug)) Logger.Debug(message);
        }

        public static void Debug(string message, params object[] args)
        {
            if (ShouldLog(NLog.LogLevel.Debug)) Logger.Debug(message, args);
        }

        public static void Debug(Exception exception, string message)
        {
            if (ShouldLog(NLog.LogLevel.Debug)) Logger.Debug(exception, message);
        }

        /// <summary>
        /// 记录Info级别日志（一般信息）
        /// </summary>
        public static void Info(string message)
        {
            if (ShouldLog(NLog.LogLevel.Info)) Logger.Info(message);
        }

        public static void Info(string message, params object[] args)
        {
            if (ShouldLog(NLog.LogLevel.Info)) Logger.Info(message, args);
        }

        public static void Info(Exception exception, string message)
        {
            if (ShouldLog(NLog.LogLevel.Info)) Logger.Info(exception, message);
        }

        /// <summary>
        /// 记录Warn级别日志（警告信息）
        /// </summary>
        public static void Warn(string message)
        {
            if (ShouldLog(NLog.LogLevel.Warn)) Logger.Warn(message);
        }

        public static void Warn(string message, params object[] args)
        {
            if (ShouldLog(NLog.LogLevel.Warn)) Logger.Warn(message, args);
        }

        public static void Warn(Exception exception, string message)
        {
            if (ShouldLog(NLog.LogLevel.Warn)) Logger.Warn(exception, message);
        }

        /// <summary>
        /// 记录Error级别日志（错误信息）
        /// </summary>
        public static void Error(string message)
        {
            if (ShouldLog(NLog.LogLevel.Error)) Logger.Error(message);
        }

        public static void Error(string message, params object[] args)
        {
            if (ShouldLog(NLog.LogLevel.Error)) Logger.Error(message, args);
        }

        public static void Error(Exception exception, string message)
        {
            if (ShouldLog(NLog.LogLevel.Error)) Logger.Error(exception, message);
        }

        /// <summary>
        /// 记录Fatal级别日志（致命错误，程序无法继续运行）
        /// </summary>
        public static void Fatal(string message)
        {
            if (ShouldLog(NLog.LogLevel.Fatal)) Logger.Fatal(message);
        }

        public static void Fatal(string message, params object[] args)
        {
            if (ShouldLog(NLog.LogLevel.Fatal)) Logger.Fatal(message, args);
        }

        public static void Fatal(Exception exception, string message)
        {
            if (ShouldLog(NLog.LogLevel.Fatal)) Logger.Fatal(exception, message);
        }
    }
}
