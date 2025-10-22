using KlarfApplication.Model;
using KlarfApplication.Service;
using System.ComponentModel;
using System.Windows.Input;
using System.IO;

namespace KlarfApplication.ViewModel
{
    /// <summary>
    /// 전체 애플리케이션의 뷰모델 (MainWindow.xaml에 바인딩)
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private KlarfModel _selectedKlarf;
        private readonly ImageService _imageService;

        #endregion

        #region Properties

        public FileListViewModel FileListViewModel { get; }
        public WaferViewModel WaferViewModel { get; }
        public DefectInfoViewModel DefectInfoViewModel { get; }
        public DefectImageViewModel DefectImageViewModel { get; }

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
            _imageService = new ImageService();

            FileListViewModel = new FileListViewModel();
            WaferViewModel = new WaferViewModel();
            DefectInfoViewModel = new DefectInfoViewModel();
            DefectImageViewModel = new DefectImageViewModel();

            // 1. 파일 선택 -> MainViewModel 업데이트
            FileListViewModel.PropertyChanged += OnFileListSelectionChanged;

            // 2. WaferMap에서 Die 선택 -> DefectInfoViewModel 업데이트
            WaferViewModel.PropertyChanged += OnWaferViewModelPropertyChanged;

            // 3. ⭐️ DefectInfo에서 Defect 선택 -> DefectImageViewModel 업데이트
            DefectInfoViewModel.PropertyChanged += OnDefectInfoPropertyChanged;
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
        private void OnWaferViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // SelectedDie 속성이 변경되었으면
            if (e.PropertyName == nameof(WaferViewModel.SelectedDie))
            {
                // WaferVM의 선택된 Die를 DefectInfoVM으로 전달
                DefectInfoViewModel.SelectedDie = WaferViewModel.SelectedDie;
            }
        }

        /// <summary>
        /// ⭐️ [수정됨] DefectInfoViewModel에서 Defect가 선택될 때 TIF 프레임을 로드합니다.
        /// </summary>
        private void OnDefectInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // DefectInfoVM에서 "SelectedDefect"가 변경되었을 때
            if (e.PropertyName == nameof(DefectInfoViewModel.SelectedDefect))
            {
                var selectedDefect = DefectInfoViewModel.SelectedDefect;

                // 선택된 Klarf 파일이나 Defect가 없으면 이미지 클리어
                if (SelectedKlarf == null || selectedDefect == null || string.IsNullOrEmpty(SelectedKlarf.TiffFileName))
                {
                    DefectImageViewModel.ClearImage();
                    return;
                }

                // ⭐️ 1. Defect에서 이미지 프레임 번호(1-based)를 가져옵니다.
                // [수정] 'ImageList' (part 17) 대신 'DefectId' (part 1)를 프레임 번호로 사용합니다.
                int frameNumber = 0;

                if (selectedDefect.DefectId != null)
                {
                    // DefectId가 "256"이라고 가정하고, 이를 정수로 파싱합니다.
                    int.TryParse(selectedDefect.DefectId, out frameNumber);
                }

                if (frameNumber <= 0)
                {
                    DefectImageViewModel.ClearImage();
                    System.Diagnostics.Debug.WriteLine($"Invalid frame number from DefectId: {selectedDefect.DefectId}");
                    return; // 유효한 프레임 번호가 없으므로 종료
                }

                try
                {
                    // 2. TIF 파일의 전체 경로를 찾습니다.
                    string klarfDirectory = Path.GetDirectoryName(SelectedKlarf.FilePath);
                    string tifPath = Path.Combine(klarfDirectory, SelectedKlarf.TiffFileName);

                    if (File.Exists(tifPath))
                    {
                        // 3. ImageService를 호출하여 TIF에서 특정 "프레임"을 로드합니다.
                        var defectImage = _imageService.LoadTifFrame(tifPath, frameNumber);

                        // 4. DefectImageViewModel에 결과 이미지를 로드합니다.
                        DefectImageViewModel.LoadDefectImage(defectImage);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"TIF file not found: {tifPath}");
                        DefectImageViewModel.ClearImage();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading TIF frame: {ex.Message}");
                    DefectImageViewModel.ClearImage();
                }
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