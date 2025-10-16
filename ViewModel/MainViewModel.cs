using KlarfApplication.Model;
using System.ComponentModel;
using System.Windows.Input;

namespace KlarfApplication.ViewModel
{
    /// <summary>
    /// 전체 애플리케이션의 뷰모델 (MainWindow.xaml에 바인딩)
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private KlarfModel _selectedKlarf;

        #endregion

        #region Properties

        public FileListViewModel FileListViewModel { get; }
        public WaferViewModel WaferViewModel { get; }
        public DefectInfoViewModel DefectInfoViewModel { get; }
        public DefectImageViewModel DefectImageViewModel { get; }

        /// <summary>
        /// 현재 선택된 Klarf 파일
        /// </summary>
        public KlarfModel SelectedKlarf
        {
            get => _selectedKlarf;
            set
            {
                if (_selectedKlarf != value)
                {
                    _selectedKlarf = value;
                    OnPropertyChanged(nameof(SelectedKlarf));
                    UpdateChildViewModels();
                }
            }
        }

        #endregion

        #region Constructors

        public MainViewModel()
        {
            FileListViewModel = new FileListViewModel();
            WaferViewModel = new WaferViewModel();
            DefectInfoViewModel = new DefectInfoViewModel();
            DefectImageViewModel = new DefectImageViewModel();

            // FileListViewModel에서 파일 선택 시 MainViewModel이 감지
            FileListViewModel.PropertyChanged += OnFileListSelectionChanged;
        }

        #endregion

        #region Private Methods

        private void OnFileListSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FileListViewModel.SelectedFile))
            {
                SelectedKlarf = FileListViewModel.SelectedFile;
            }
        }

        private void UpdateChildViewModels()
        {
            WaferViewModel.UpdateFromKlarf(SelectedKlarf);
            DefectInfoViewModel.UpdateFromKlarf(SelectedKlarf);
            DefectImageViewModel.UpdateFromKlarf(SelectedKlarf);
        }

        #endregion
    }
}
