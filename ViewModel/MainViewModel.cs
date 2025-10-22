using KlarfApplication.Model;
using KlarfApplication.Service;
using System.ComponentModel;
using System.Windows.Input;
using System.IO;
using System.Linq;

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

            // 1. 파일 선택
            FileListViewModel.PropertyChanged += OnFileListSelectionChanged;

            // 2. Die 선택 (Wafer -> DefectInfo)
            WaferViewModel.PropertyChanged += OnWaferViewModelPropertyChanged;

            // 3. Defect 선택 (DefectInfo -> Image)
            DefectInfoViewModel.PropertyChanged += OnDefectInfoPropertyChanged;

            // 4. (오른쪽) 불량 Die 탐색 요청 (DefectInfo -> Main)
            DefectInfoViewModel.DieNavigationRequested += OnDieNavigationRequested;

            // 5. ⭐️ [신규] (가운데) 모든 Die 탐색 요청 (DefectInfo -> Main)
            DefectInfoViewModel.AllDieNavigationRequested += OnAllDieNavigationRequested;
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
            if (e.PropertyName == nameof(WaferViewModel.SelectedDie))
            {
                DefectInfoViewModel.SelectedDie = WaferViewModel.SelectedDie;
            }
        }

        private void OnDefectInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // (이미지 로드 로직 - 변경 없음)
            if (e.PropertyName == nameof(DefectInfoViewModel.SelectedDefect))
            {
                var selectedDefect = DefectInfoViewModel.SelectedDefect;

                if (SelectedKlarf == null || selectedDefect == null || string.IsNullOrEmpty(SelectedKlarf.TiffFileName))
                {
                    DefectImageViewModel.ClearImage();
                    return;
                }

                int frameNumber = 0;
                if (selectedDefect.DefectId != null)
                {
                    int.TryParse(selectedDefect.DefectId, out frameNumber);
                }

                if (frameNumber <= 0)
                {
                    DefectImageViewModel.ClearImage();
                    System.Diagnostics.Debug.WriteLine($"Invalid frame number from DefectId: {selectedDefect.DefectId}");
                    return;
                }

                try
                {
                    string klarfDirectory = Path.GetDirectoryName(SelectedKlarf.FilePath);
                    string tifPath = Path.Combine(klarfDirectory, SelectedKlarf.TiffFileName);

                    if (File.Exists(tifPath))
                    {
                        var defectImage = _imageService.LoadTifFrame(tifPath, frameNumber);
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

        /// <summary>
        /// (오른쪽) "불량" Die 탐색 요청 처리
        /// </summary>
        private void OnDieNavigationRequested(DieNavigationType navType)
        {
            var defectiveDies = WaferViewModel.Dies.Where(d => !d.IsGood).ToList();
            if (defectiveDies.Count == 0) return;

            var currentDie = WaferViewModel.SelectedDie;
            int currentIndex = (currentDie == null) ? -1 : defectiveDies.IndexOf(currentDie);

            DieViewModel newSelectedDie = null;

            switch (navType)
            {
                case DieNavigationType.First:
                    newSelectedDie = defectiveDies.First();
                    break;
                case DieNavigationType.Previous:
                    if (currentIndex > 0)
                        newSelectedDie = defectiveDies[currentIndex - 1];
                    else
                        newSelectedDie = defectiveDies.First();
                    break;
                case DieNavigationType.Next:
                    if (currentIndex >= 0 && currentIndex < defectiveDies.Count - 1)
                        newSelectedDie = defectiveDies[currentIndex + 1];
                    else
                        newSelectedDie = defectiveDies.Last();
                    break;
                case DieNavigationType.Last:
                    newSelectedDie = defectiveDies.Last();
                    break;
            }

            if (newSelectedDie != null)
            {
                WaferViewModel.SelectedDie = newSelectedDie;
            }
        }

        /// <summary>
        /// ⭐️ [신규] (가운데) "모든" Die 탐색 요청 처리
        /// </summary>
        private void OnAllDieNavigationRequested(AllDieNavigationType navType)
        {
            // 1. WaferViewModel에서 "모든" Die 리스트를 가져옴
            var allDies = WaferViewModel.Dies.ToList();
            if (allDies.Count == 0) return;

            // 2. 현재 선택된 Die의 인덱스를 찾음
            var currentDie = WaferViewModel.SelectedDie;
            int currentIndex = (currentDie == null) ? -1 : allDies.IndexOf(currentDie);

            DieViewModel newSelectedDie = null;

            // 3. 요청 타입에 따라 다음/이전 Die를 결정
            switch (navType)
            {
                case AllDieNavigationType.First:
                    newSelectedDie = allDies.First();
                    break;
                case AllDieNavigationType.Previous:
                    if (currentIndex > 0)
                        newSelectedDie = allDies[currentIndex - 1];
                    else
                        newSelectedDie = allDies.First();
                    break;
                case AllDieNavigationType.Next:
                    if (currentIndex >= 0 && currentIndex < allDies.Count - 1)
                        newSelectedDie = allDies[currentIndex + 1];
                    else
                        newSelectedDie = allDies.Last();
                    break;
                case AllDieNavigationType.Last:
                    newSelectedDie = allDies.Last();
                    break;
            }

            // 4. WaferViewModel의 SelectedDie를 업데이트 (이후 자동 연쇄 반응)
            if (newSelectedDie != null)
            {
                WaferViewModel.SelectedDie = newSelectedDie;
            }
        }


        private void UpdateChildViewModels()
        {
            WaferViewModel.UpdateFromKlarf(SelectedKlarf);

            // ⭐️ DefectInfoViewModel에 Klarf 파일과 "모든" Die 리스트를 모두 전달
            DefectInfoViewModel.UpdateFromKlarf(SelectedKlarf);
            DefectInfoViewModel.UpdateDieList(WaferViewModel.Dies);

            DefectImageViewModel.UpdateFromKlarf(SelectedKlarf);
        }

        #endregion
    }
}