//using KlarfApplication.Service;
//using KlarfApplication.Utility;
//using Microsoft.Win32;
//using System;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Windows.Input;

//namespace KlarfApplication.ViewModel
//{
//    public class LogViewModel : ViewModelBase
//    {
//        private readonly LogService _logService;
//        private ObservableCollection<LogEntry> _filteredLogs;
//        private bool _showDebug = true;
//        private bool _showInfo = true;
//        private bool _showWarning = true;
//        private bool _showError = true;
//        private bool _autoScroll = true;
//        private string _statusMessage;

//        public ObservableCollection<LogEntry> FilteredLogs
//        {
//            get => _filteredLogs;
//            set
//            {
//                _filteredLogs = value;
//                OnPropertyChanged(nameof(FilteredLogs));
//                OnPropertyChanged(nameof(FilteredLogCount));
//            }
//        }

//        public bool ShowDebug
//        {
//            get => _showDebug;
//            set
//            {
//                _showDebug = value;
//                OnPropertyChanged(nameof(ShowDebug));
//                ApplyFilter();
//            }
//        }

//        public bool ShowInfo
//        {
//            get => _showInfo;
//            set
//            {
//                _showInfo = value;
//                OnPropertyChanged(nameof(ShowInfo));
//                ApplyFilter();
//            }
//        }

//        public bool ShowWarning
//        {
//            get => _showWarning;
//            set
//            {
//                _showWarning = value;
//                OnPropertyChanged(nameof(ShowWarning));
//                ApplyFilter();
//            }
//        }

//        public bool ShowError
//        {
//            get => _showError;
//            set
//            {
//                _showError = value;
//                OnPropertyChanged(nameof(ShowError));
//                ApplyFilter();
//            }
//        }

//        public bool AutoScroll
//        {
//            get => _autoScroll;
//            set
//            {
//                _autoScroll = value;
//                OnPropertyChanged(nameof(AutoScroll));
//            }
//        }

//        public string StatusMessage
//        {
//            get => _statusMessage;
//            set
//            {
//                _statusMessage = value;
//                OnPropertyChanged(nameof(StatusMessage));
//            }
//        }

//        public int LogCount => _logService.Logs.Count;
//        public int FilteredLogCount => FilteredLogs?.Count ?? 0;

//        public ICommand RefreshCommand { get; }
//        public ICommand ClearCommand { get; }
//        public ICommand ExportCommand { get; }

//        public LogViewModel()
//        {
//            _logService = LogService.Instance;

//            RefreshCommand = new RelayCommand(Refresh);
//            ClearCommand = new RelayCommand(Clear);
//            ExportCommand = new RelayCommand(Export);

//            // 로그 변경 감지
//            _logService.Logs.CollectionChanged += (s, e) =>
//            {
//                OnPropertyChanged(nameof(LogCount));
//                ApplyFilter();
//            };

//            ApplyFilter();
//            StatusMessage = "Ready";
//        }

//        private void Refresh()
//        {
//            ApplyFilter();
//            StatusMessage = $"Refreshed at {DateTime.Now:HH:mm:ss}";
//        }

//        private void Clear()
//        {
//            _logService.Clear();
//            StatusMessage = "Logs cleared";
//        }

//        private void Export()
//        {
//            var dialog = new SaveFileDialog
//            {
//                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
//                FileName = $"log_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
//            };

//            if (dialog.ShowDialog() == true)
//            {
//                try
//                {
//                    _logService.ExportToFile(dialog.FileName);
//                    StatusMessage = $"Exported to {dialog.FileName}";
//                }
//                catch (Exception ex)
//                {
//                    StatusMessage = $"Export failed: {ex.Message}";
//                }
//            }
//        }

//        private void ApplyFilter()
//        {
//            var filtered = _logService.Logs.Where(log =>
//            {
//                return log.Level switch
//                {
//                    LogLevel.Debug => ShowDebug,
//                    LogLevel.Info => ShowInfo,
//                    LogLevel.Warning => ShowWarning,
//                    LogLevel.Error => ShowError,
//                    _ => true
//                };
//            });

//            FilteredLogs = new ObservableCollection<LogEntry>(filtered);
//        }
//    }
//}