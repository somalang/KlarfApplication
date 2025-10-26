using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace KlarfApplication.Service
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }

        public string FormattedMessage => $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] [{Source}] {Message}";
    }

    public class LogService
    {
        private static LogService _instance;
        private static readonly object _lock = new object();

        public static LogService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LogService();
                        }
                    }
                }
                return _instance;
            }
        }

        private readonly ObservableCollection<LogEntry> _logs;
        private readonly int _maxLogEntries = 1000; // 메모리 관리를 위한 최대 로그 개수
        private bool _enableFileLogging = false;
        private string _logFilePath;

        public ObservableCollection<LogEntry> Logs => _logs;

        private LogService()
        {
            _logs = new ObservableCollection<LogEntry>();
            InitializeFileLogging();
        }

        private void InitializeFileLogging()
        {
            try
            {
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "KlarfApplication",
                    "Logs"
                );

                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }

                _logFilePath = Path.Combine(appDataPath, $"log_{DateTime.Now:yyyyMMdd}.txt");
                _enableFileLogging = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize file logging: {ex.Message}");
                _enableFileLogging = false;
            }
        }

        public void WriteDebug(string message, string source = "")
        {
            Write(LogLevel.Debug, message, source);
        }

        public void WriteInfo(string message, string source = "")
        {
            Write(LogLevel.Info, message, source);
        }

        public void WriteWarning(string message, string source = "")
        {
            Write(LogLevel.Warning, message, source);
        }

        public void WriteError(string message, string source = "")
        {
            Write(LogLevel.Error, message, source);
        }

        public void WriteError(Exception ex, string context = "", string source = "")
        {
            string message = string.IsNullOrEmpty(context)
                ? $"{ex.GetType().Name}: {ex.Message}"
                : $"{context} - {ex.GetType().Name}: {ex.Message}";

            Write(LogLevel.Error, message, source);
        }

        private void Write(LogLevel level, string message, string source)
        {
            try
            {
                var entry = new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = level,
                    Message = message,
                    Source = string.IsNullOrEmpty(source) ? "Application" : source
                };

                // UI 스레드에서 실행
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    _logs.Add(entry);

                    // 최대 개수 제한
                    if (_logs.Count > _maxLogEntries)
                    {
                        _logs.RemoveAt(0);
                    }
                });

                // 파일에 기록
                if (_enableFileLogging)
                {
                    WriteToFile(entry);
                }

                // 콘솔/디버그 출력
                System.Diagnostics.Debug.WriteLine(entry.FormattedMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        private void WriteToFile(LogEntry entry)
        {
            try
            {
                File.AppendAllText(_logFilePath, entry.FormattedMessage + Environment.NewLine);
            }
            catch
            {
                // 파일 쓰기 실패는 무시 (로그 시스템이 앱을 중단시키면 안 됨)
            }
        }

        public void Clear()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                _logs.Clear();
            });
            WriteInfo("Log cleared", "LogService");
        }

        public void ExportToFile(string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var log in _logs)
                    {
                        writer.WriteLine(log.FormattedMessage);
                    }
                }
                WriteInfo($"Logs exported to {filePath}", "LogService");
            }
            catch (Exception ex)
            {
                WriteError(ex, "Failed to export logs", "LogService");
                throw;
            }
        }
    }
}