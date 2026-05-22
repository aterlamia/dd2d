using Godot;

namespace dd2d.core
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    public static class Log
    {
        public static LogLevel MinimumLevel { get; set; } = LogLevel.Debug;
        public static bool Enabled { get; set; } = true;

        public static void Debug(string message, string category = "")
        {
            LogMessage(LogLevel.Debug, message, category);
        }

        public static void Info(string message, string category = "")
        {
            LogMessage(LogLevel.Info, message, category);
        }

        public static void Warning(string message, string category = "")
        {
            LogMessage(LogLevel.Warning, message, category);
        }

        public static void Error(string message, string category = "")
        {
            LogMessage(LogLevel.Error, message, category);
        }

        private static void LogMessage(LogLevel level, string message, string category)
        {
            if (!Enabled || level < MinimumLevel)
                return;

            string prefix = string.IsNullOrEmpty(category) ? "" : $"[{category}] ";
            string fullMessage = $"{prefix}{message}";

            if (level >= LogLevel.Error)
                GD.PrintErr(fullMessage);
            else
                GD.Print(fullMessage);
        }
    }
}
