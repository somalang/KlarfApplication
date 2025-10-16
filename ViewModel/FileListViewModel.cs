using KlarfApplication.Model;
using KlarfApplication.Service;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace KlarfApplication.ViewModel
{
    /// <summary>
    /// 파일 리스트 뷰어의 ViewModel.
    /// KLARF 파일을 불러오고 리스트에 표시하며, 선택/삭제/갱신 기능을 제공합니다.
    /// </summary>
    public class FileListViewModel : ViewModelBase
    {
        #region Fields

        private readonly KlarfService _klarfService;
        private ObservableCollection<KlarfModel> _fileList;
        private KlarfModel _selectedFile;
        private Visibility _noFilesVisibility;

        #endregion

        #region Properties

        public ObservableCollection<KlarfModel> FileList
        {
            get => _fileList;
            set
            {
                _fileList = value;
                OnPropertyChanged(nameof(FileList));
            }
        }

        public KlarfModel SelectedFile
        {
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                OnPropertyChanged(nameof(SelectedFile));
            }
        }

        public Visibility NoFilesVisibility
        {
            get => _noFilesVisibility;
            set
            {
                _noFilesVisibility = value;
                OnPropertyChanged(nameof(NoFilesVisibility));
            }
        }

        #endregion

        #region Commands

        public ICommand OpenFileCommand { get; }
        public ICommand ClearFilesCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Constructors

        public FileListViewModel()
        {
            _klarfService = new KlarfService();
            FileList = new ObservableCollection<KlarfModel>();
            NoFilesVisibility = Visibility.Visible;

            OpenFileCommand = new RelayCommand(OpenFile);
            ClearFilesCommand = new RelayCommand(ClearFiles);
            RefreshCommand = new RelayCommand(RefreshFiles);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// KLARF 파일을 열고 리스트에 추가합니다.
        /// </summary>
        private void OpenFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "KLARF Files (*.klarf)|*.klarf|All Files (*.*)|*.*",
                Title = "Select KLARF File"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var klarf = _klarfService.LoadKlarf(dialog.FileName);

                    // 이미 같은 파일이 열려 있다면 중복 추가 방지
                    if (FileList.Any(f => f.FilePath == klarf.FilePath))
                        return;

                    FileList.Add(klarf);
                    NoFilesVisibility = FileList.Any() ? Visibility.Collapsed : Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일을 여는 중 오류가 발생했습니다.\n\n{ex.Message}",
                        "File Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 파일 리스트를 모두 제거합니다.
        /// </summary>
        private void ClearFiles()
        {
            FileList.Clear();
            NoFilesVisibility = Visibility.Visible;
        }

        /// <summary>
        /// 파일 리스트를 새로 고칩니다.
        /// </summary>
        private void RefreshFiles()
        {
            foreach (var file in FileList.ToList())
            {
                if (File.Exists(file.FilePath))
                {
                    var refreshed = _klarfService.LoadKlarf(file.FilePath);
                    var index = FileList.IndexOf(file);
                    FileList[index] = refreshed;
                }
            }

            NoFilesVisibility = FileList.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion
    }
}
