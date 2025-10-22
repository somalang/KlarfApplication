using KlarfApplication.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using KlarfApplication;
using System.Collections.Generic;

namespace KlarfApplication.ViewModel
{
    // (Enum 선언 등은 변경 없음)
    public enum DieNavigationType { First, Previous, Next, Last }
    public enum AllDieNavigationType { First, Previous, Next, Last }

    public class DefectInfoViewModel : ViewModelBase
    {
        // (필드 선언 등은 변경 없음)
        private KlarfModel _currentKlarfFile;
        private DieViewModel _selectedDie;
        private Defect _selectedDefect;
        private ObservableCollection<Defect> _allDefects;
        private ObservableCollection<Defect> _defects;
        private List<DieViewModel> _allDefectiveDies = new List<DieViewModel>();
        private List<DieViewModel> _allWaferDies = new List<DieViewModel>();

        public event Action<DieNavigationType> DieNavigationRequested;
        public event Action<AllDieNavigationType> AllDieNavigationRequested;

        public ICommand FirstDieCommand { get; }
        public ICommand PreviousDieCommand { get; }
        public ICommand NextDieCommand { get; }
        public ICommand LastDieCommand { get; }

        public ICommand FirstDefectCommand { get; }
        public ICommand PreviousDefectCommand { get; }
        public ICommand NextDefectCommand { get; }
        public ICommand LastDefectCommand { get; }

        public ICommand FirstAllDieCommand { get; }
        public ICommand PreviousAllDieCommand { get; }
        public ICommand NextAllDieCommand { get; }
        public ICommand LastAllDieCommand { get; }

        private string _currentDieInfo;
        public string CurrentDieInfo
        {
            get => _currentDieInfo;
            set { _currentDieInfo = value; OnPropertyChanged(nameof(CurrentDieInfo)); }
        }

        private string _currentDefectInfo;
        public string CurrentDefectInfo
        {
            get => _currentDefectInfo;
            set { _currentDefectInfo = value; OnPropertyChanged(nameof(CurrentDefectInfo)); }
        }

        private string _currentAllDieInfo;
        public string CurrentAllDieInfo
        {
            get => _currentAllDieInfo;
            set { _currentAllDieInfo = value; OnPropertyChanged(nameof(CurrentAllDieInfo)); }
        }

        // (생성자, Command 초기화 등은 변경 없음)
        public DefectInfoViewModel()
        {
            // 1. (왼쪽) 로컬 Defect
            FirstDieCommand = new RelayCommand(NavigateFirstLocalDefect, CanNavigateLocalDefects);
            PreviousDieCommand = new RelayCommand(NavigatePreviousLocalDefect, CanNavigateLocalDefects);
            NextDieCommand = new RelayCommand(NavigateNextLocalDefect, CanNavigateLocalDefects);
            LastDieCommand = new RelayCommand(NavigateLastLocalDefect, CanNavigateLocalDefects);

            // 2. (오른쪽) 불량 Die
            FirstDefectCommand = new RelayCommand(() => DieNavigationRequested?.Invoke(DieNavigationType.First), CanNavigateGlobalDies);
            PreviousDefectCommand = new RelayCommand(() => DieNavigationRequested?.Invoke(DieNavigationType.Previous), CanNavigateGlobalDies);
            NextDefectCommand = new RelayCommand(() => DieNavigationRequested?.Invoke(DieNavigationType.Next), CanNavigateGlobalDies);
            LastDefectCommand = new RelayCommand(() => DieNavigationRequested?.Invoke(DieNavigationType.Last), CanNavigateGlobalDies);

            // 3. (가운데) 모든 Die
            FirstAllDieCommand = new RelayCommand(() => AllDieNavigationRequested?.Invoke(AllDieNavigationType.First), CanNavigateAllDies);
            PreviousAllDieCommand = new RelayCommand(() => AllDieNavigationRequested?.Invoke(AllDieNavigationType.Previous), CanNavigateAllDies);
            NextAllDieCommand = new RelayCommand(() => AllDieNavigationRequested?.Invoke(AllDieNavigationType.Next), CanNavigateAllDies);
            LastAllDieCommand = new RelayCommand(() => AllDieNavigationRequested?.Invoke(AllDieNavigationType.Last), CanNavigateAllDies);


            // 초기화
            _allDefects = new ObservableCollection<Defect>();
            Defects = new ObservableCollection<Defect>();
            UpdateDieNavInfo();
            UpdateDefectNavInfo();
            UpdateAllDieNavInfo();
        }

        // (Properties: Defects, SelectedDie, SelectedDefect 등 변경 없음)
        public ObservableCollection<Defect> Defects
        {
            get => _defects;
            set
            {
                _defects = value;
                OnPropertyChanged(nameof(Defects));
                OnPropertyChanged(nameof(NoDefectsVisibility));
            }
        }

        public DieViewModel SelectedDie
        {
            get => _selectedDie;
            set
            {
                if (_selectedDie != value)
                {
                    _selectedDie = value;
                    OnPropertyChanged(nameof(SelectedDie));
                    OnPropertyChanged(nameof(SelectedDieVisibility));

                    FilterDefectsBySelectedDie();

                    UpdateDieNavInfo();
                    UpdateDefectNavInfo();
                    UpdateAllDieNavInfo();
                }
            }
        }

        public Defect SelectedDefect
        {
            get => _selectedDefect;
            set
            {
                _selectedDefect = value;
                OnPropertyChanged(nameof(SelectedDefect));
                UpdateDieNavInfo(); // (왼쪽) 텍스트만 업데이트
            }
        }

        public KlarfModel CurrentKlarfFile
        {
            get => _currentKlarfFile;
            private set
            {
                _currentKlarfFile = value;
                OnPropertyChanged(nameof(CurrentKlarfFile));
                OnPropertyChanged(nameof(DefectCount));
            }
        }

        public Visibility NoDefectsVisibility => (Defects == null || Defects.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SelectedDieVisibility => _selectedDie == null ? Visibility.Collapsed : Visibility.Visible;
        public int DefectCount => CurrentKlarfFile?.TotalDefectCount ?? 0;

        public void UpdateFromKlarf(KlarfModel klarf)
        {
            CurrentKlarfFile = klarf;
            if (klarf == null)
            {
                _allDefects = new ObservableCollection<Defect>();
                Defects = new ObservableCollection<Defect>();
                SelectedDie = null;
            }
            else
            {
                _allDefects = new ObservableCollection<Defect>(klarf.Defects);
                Defects = new ObservableCollection<Defect>(_allDefects);
            }
            UpdateDieNavInfo();
        }

        public void UpdateDieList(ObservableCollection<DieViewModel> allWaferDies)
        {
            if (allWaferDies == null)
            {
                _allDefectiveDies = new List<DieViewModel>();
                _allWaferDies = new List<DieViewModel>();
            }
            else
            {
                _allDefectiveDies = allWaferDies.Where(d => !d.IsGood).ToList();
                _allWaferDies = allWaferDies.ToList();
            }

            UpdateDieNavInfo();
            UpdateDefectNavInfo();
            UpdateAllDieNavInfo();

            RaiseCanExecuteChanged();
        }

        private void FilterDefectsBySelectedDie()
        {
            if (_selectedDie == null)
            {
                Defects = new ObservableCollection<Defect>(_allDefects ?? new ObservableCollection<Defect>());
            }
            else
            {
                var filtered = _allDefects.Where(d =>
                    d.Row == _selectedDie.Row &&
                    d.Column == _selectedDie.Column);
                Defects = new ObservableCollection<Defect>(filtered);
            }

            SelectedDefect = Defects.FirstOrDefault();
        }

        // --- 1. (왼쪽) 로컬 Defect (CanExecute/Navigate 로직 변경 없음) ---
        private bool CanNavigateLocalDefects() => Defects != null && Defects.Count > 0;
        private void NavigateFirstLocalDefect()
        {
            if (CanNavigateLocalDefects()) SelectedDefect = Defects.First();
        }
        private void NavigatePreviousLocalDefect()
        {
            if (!CanNavigateLocalDefects()) return;
            int currentIndex = Defects.IndexOf(SelectedDefect);
            if (currentIndex > 0) SelectedDefect = Defects[currentIndex - 1];
        }
        private void NavigateNextLocalDefect()
        {
            if (!CanNavigateLocalDefects()) return;
            int currentIndex = Defects.IndexOf(SelectedDefect);
            if (currentIndex < Defects.Count - 1) SelectedDefect = Defects[currentIndex + 1];
        }
        private void NavigateLastLocalDefect()
        {
            if (CanNavigateLocalDefects()) SelectedDefect = Defects.Last();
        }

        // --- 2. (오른쪽) 불량 Die (CanExecute 로직 변경 없음) ---
        private bool CanNavigateGlobalDies() => _allDefectiveDies != null && _allDefectiveDies.Count > 0;

        // --- 3. (가운데) 모든 Die (CanExecute 로직 변경 없음) ---
        private bool CanNavigateAllDies() => _allWaferDies != null && _allWaferDies.Count > 0;


        // --- [신규] 텍스트 업데이트 로직 3가지 ---

        /// 1. (왼쪽) "defects per die"
        /// ⭐️ [수정] 사용자의 요청("선택된 게 없거나 good die인 경우 - 표시")에 맞게 로직 수정
        private void UpdateDieNavInfo()
        {
            var list = Defects; // 현재 DataGrid에 표시된 리스트
            var total = (list?.Count ?? 0);

            // 요청 1: "Good Die인 경우" (리스트가 비어있음)
            if (SelectedDie != null && SelectedDie.IsGood)
            {
                CurrentDieInfo = "- / 0";
            }
            // 요청 2: "선택된 게 없는 경우" (SelectedDie가 null)
            else if (SelectedDie == null)
            {
                // 이 경우 'Defects' 리스트는 모든 Defect를 표시합니다.
                CurrentDieInfo = $"- / {total}";
            }
            // 그 외 (불량 Die가 선택된 경우)
            else
            {
                var currentItem = SelectedDefect;
                int currentIndex = (currentItem == null || list == null) ? -1 : list.IndexOf(currentItem);

                if (currentIndex == -1)
                {
                    CurrentDieInfo = $"- / {total}";
                }
                else
                {
                    CurrentDieInfo = $"{currentIndex + 1} / {total}";
                }
            }
            RaiseCanExecuteChanged();
        }


        /// 2. (오른쪽) "die with defects" (기존 로직이 이미 사용자의 요구사항과 일치)
        private void UpdateDefectNavInfo()
        {
            var list = _allDefectiveDies;
            var total = (list?.Count ?? 0);

            if (total == 0)
            {
                CurrentDefectInfo = "0 / 0";
                RaiseCanExecuteChanged(); // ⭐️ 버튼 비활성화를 위해 추가
                return;
            }

            var currentItem = SelectedDie; // 항목 = Die
            int currentIndex = (currentItem == null || list == null) ? -1 : list.IndexOf(currentItem);

            if (currentIndex == -1) // ⭐️ 선택된 게 없거나(null) Good Die일 때(Not in list)
            {
                CurrentDefectInfo = $"- / {total}";
            }
            else
            {
                CurrentDefectInfo = $"{currentIndex + 1} / {total}";
            }
            RaiseCanExecuteChanged();
        }

        /// 3. (가운데) "All Dies" (기존 로직이 이미 사용자의 요구사항과 일치)
        private void UpdateAllDieNavInfo()
        {
            var list = _allWaferDies;
            var total = (list?.Count ?? 0);

            if (total == 0)
            {
                CurrentAllDieInfo = "0 / 0";
                RaiseCanExecuteChanged(); // ⭐️ 버튼 비활성화를 위해 추가
                return;
            }

            var currentItem = SelectedDie; // 항목 = Die
            int currentIndex = (currentItem == null || list == null) ? -1 : list.IndexOf(currentItem);

            if (currentIndex == -1) // ⭐️ 선택된 게 없을 때(null)
            {
                CurrentAllDieInfo = $"- / {total}";
            }
            else
            {
                CurrentAllDieInfo = $"{currentIndex + 1} / {total}";
            }
            RaiseCanExecuteChanged();
        }


        // (RaiseCanExecuteChanged 메서드 변경 없음)
        private void RaiseCanExecuteChanged()
        {
            // 1. 왼쪽
            (FirstDieCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PreviousDieCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NextDieCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (LastDieCommand as RelayCommand)?.RaiseCanExecuteChanged();

            // 2. 오른쪽
            (FirstDefectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PreviousDefectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NextDefectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (LastDefectCommand as RelayCommand)?.RaiseCanExecuteChanged();

            // 3. 가운데
            (FirstAllDieCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PreviousAllDieCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NextAllDieCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (LastAllDieCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}