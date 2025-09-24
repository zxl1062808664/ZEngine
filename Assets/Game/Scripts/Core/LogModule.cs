using UnityEngine;

namespace Framework.Core
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public class LogModule : IModule
    {
        private GameFramework _gameFramework;
        private LogLevel _logLevel = LogLevel.Debug;
        private bool _logToFile = false;
        private string _logFilePath;

        public void SetGameFramework(GameFramework gameFramework)
        {
            _gameFramework = gameFramework;
        }

        public void OnInitialize()
        {
            _logFilePath = Application.persistentDataPath + "/logs/";
            System.IO.Directory.CreateDirectory(_logFilePath);
            _logFilePath += System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log";

            Log("Log module initialized");
        }

        public void SetLogLevel(LogLevel level)
        {
            _logLevel = level;
        }

        public void SetLogToFile(bool enable)
        {
            _logToFile = enable;
        }

        public static void Debug(string message)
        {
            Instance?.InternalLog(LogLevel.Debug, message);
        }

        public static void Log(string message)
        {
            Instance?.InternalLog(LogLevel.Info, message);
        }

        public static void Warning(string message)
        {
            Instance?.InternalLog(LogLevel.Warning, message);
        }

        public static void Error(string message)
        {
            Instance?.InternalLog(LogLevel.Error, message);
        }

        public static void Fatal(string message)
        {
            Instance?.InternalLog(LogLevel.Fatal, message);
        }

        private void InternalLog(LogLevel level, string message)
        {
            if (level < _logLevel)
                return;

            string logMessage = $"[{System.DateTime.Now:HH:mm:ss}] [{level}] {message}";

            // 输出到Unity控制台
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    UnityEngine.Debug.Log(logMessage);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(logMessage);
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    UnityEngine.Debug.LogError(logMessage);
                    break;
            }

            // 输出到文件
            if (_logToFile)
            {
                try
                {
                    System.IO.File.AppendAllText(_logFilePath, logMessage + "\n");
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"Failed to write log to file: {e.Message}");
                }
            }
        }

        public void OnUpdate(float deltaTime) { }
        public void OnFixedUpdate(float fixedDeltaTime) { }
        public void OnLateUpdate(float deltaTime) { }

        public void OnShutdown()
        {
            Log("Log module shutdown");
        }

        internal static LogModule Instance => GameFramework.Instance?.GetModule<LogModule>();
    }
}
