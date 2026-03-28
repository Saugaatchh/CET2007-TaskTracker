namespace TrackerApp.Core.Logging
{
    /// <summary>
    /// Singleton pattern: Guarantees only one Logger instance exists across the entire application.
    /// All subsystems share the same logger, ensuring a single, consistent audit trail written to a .txt file.
    /// Thread-safe using a lock object.
    /// </summary>
    public sealed class AppLogger
    {
        private static AppLogger? _instance;
        private static readonly object _lock = new();
        private readonly string _logFilePath;

        // Private constructor prevents direct instantiation
        private AppLogger(string logDirectory)
        {
            Directory.CreateDirectory(logDirectory);
            _logFilePath = Path.Combine(logDirectory, "app.log");
        }

        /// <summary>
        /// Returns the single AppLogger instance, creating it on first access (lazy init).
        /// </summary>
        public static AppLogger GetInstance(string logDirectory = "data")
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new AppLogger(logDirectory);
                }
            }
            return _instance;
        }

        /// <summary>Writes an informational log entry.</summary>
        public void Info(string message) => WriteLog("INFO ", message);

        /// <summary>Writes a warning log entry.</summary>
        public void Warn(string message) => WriteLog("WARN ", message);

        /// <summary>Writes an error log entry.</summary>
        public void Error(string message) => WriteLog("ERROR", message);

        private void WriteLog(string level, string message)
        {
            string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            Console.ForegroundColor = level.Trim() switch
            {
                "WARN"  => ConsoleColor.Yellow,
                "ERROR" => ConsoleColor.Red,
                _       => ConsoleColor.DarkGray
            };
            Console.WriteLine(entry);
            Console.ResetColor();

            lock (_lock)
            {
                // Log rotation could be added here in a future version
                File.AppendAllText(_logFilePath, entry + Environment.NewLine);
            }
        }
    }
}
