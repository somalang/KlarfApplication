using KlarfApplication.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System;

namespace KlarfApplication.ViewModel
{
    public enum DieNavigationType { First, Previous, Next, Last }
    public enum AllDieNavigationType { First, Previous, Next, Last }

    public class DefectInfoViewModel : ViewModelBase
    {
        private KlarfModel _currentKlarfFile;
        private DieViewModel _selectedDie;
        private Defect _selectedDefect;
        private ObservableCollection<Defect> _allDefects;
        private ObservableCollection<Defect> _defects;
        private List<DieViewModel> _allDefectiveDies = new();
        private List<DieViewModel> _allWaferDies = new();

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

        public DefectInfoViewModel()
        {
            // 1️⃣ (왼쪽) 로컬 Defect
            FirstDieCommand = new RelayCommand(NavigateFirstLocalDefect, CanNavigateLocalDefects);
            PreviousDieCommand = new RelayCommand(NavigatePreviousLocalDefect, CanNavigateLocalDefects);
            NextDieCommand = new RelayCommand(NavigateNextLocalDefect, CanNavigateLocalDefects);
            LastDieCommand = new RelayCommand(NavigateLastLocalDefect, CanNavigateLocalDefects);

            // 2️⃣ (오른쪽) 불량 Die
            FirstDefectCommand = new RelayCommand(() => NavigateGlobalDies(DieNavigationType.First), CanNavigateGlobalDies);
            PreviousDefectCommand = new RelayCommand(() => NavigateGlobalDies(DieNavigationType.Previous), CanNavigateGlobalDies);
            NextDefectCommand = new RelayCommand(() => NavigateGlobalDies(DieNavigationType.Next), CanNavigateGlobalDies);
            LastDefectCommand = new RelayCommand(() => NavigateGlobalDies(DieNavigationType.Last), CanNavigateGlobalDies);

            // 3️⃣ (가운데) 전체 Die
            FirstAllDieCommand = new RelayCommand(() => NavigateAllDies(AllDieNavigationType.First), CanNavigateAllDies);
            PreviousAllDieCommand = new RelayCommand(() => NavigateAllDies(AllDieNavigationType.Previous), CanNavigateAllDies);
            NextAllDieCommand = new RelayCommand(() => NavigateAllDies(AllDieNavigationType.Next), CanNavigateAllDies);
            LastAllDieCommand = new RelayCommand(() => NavigateAllDies(AllDieNavigationType.Last), CanNavigateAllDies);

            _allDefects = new ObservableCollection<Defect>();
            Defects = new ObservableCollection<Defect>();
            UpdateDieNavInfo();
            UpdateDefectNavInfo();
            UpdateAllDieNavInfo();
        }

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
                UpdateDieNavInfo();
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
                _allDefectiveDies = new();
                _allWaferDies = new();
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

        // ------------------- 1️⃣ Local Defects -------------------
        private bool CanNavigateLocalDefects() => Defects != null;

        private void NavigateFirstLocalDefect()
        {
            if (!CanNavigateLocalDefects() || Defects.Count == 0) return;
            SelectedDefect = Defects.First();
        }

        private void NavigatePreviousLocalDefect()
        {
            if (!CanNavigateLocalDefects() || Defects.Count == 0) return;
            int currentIndex = Defects.IndexOf(SelectedDefect);
            if (currentIndex > 0) SelectedDefect = Defects[currentIndex - 1];
        }

        private void NavigateNextLocalDefect()
        {
            if (!CanNavigateLocalDefects() || Defects.Count == 0) return;
            int currentIndex = Defects.IndexOf(SelectedDefect);
            if (currentIndex < Defects.Count - 1) SelectedDefect = Defects[currentIndex + 1];
        }

        private void NavigateLastLocalDefect()
        {
            if (!CanNavigateLocalDefects() || Defects.Count == 0) return;
            SelectedDefect = Defects.Last();
        }

        // ------------------- 2️⃣ Defective Dies -------------------
        private bool CanNavigateGlobalDies() => Defects != null;

        private void NavigateGlobalDies(DieNavigationType type)
        {
            if (!CanNavigateGlobalDies()) return;

            // ⭐ 선택된 게 없을 때 → 첫 번째로 자동 이동
            if (SelectedDie == null)
            {
                SelectedDie = _allDefectiveDies.FirstOrDefault();
                return;
            }

            int index = _allDefectiveDies.IndexOf(SelectedDie);
            if (index < 0) index = 0;

            switch (type)
            {
                case DieNavigationType.First:
                    SelectedDie = _allDefectiveDies.FirstOrDefault();
                    break;
                case DieNavigationType.Previous:
                    if (index > 0) SelectedDie = _allDefectiveDies[index - 1];
                    break;
                case DieNavigationType.Next:
                    if (index < _allDefectiveDies.Count - 1) SelectedDie = _allDefectiveDies[index + 1];
                    break;
                case DieNavigationType.Last:
                    SelectedDie = _allDefectiveDies.LastOrDefault();
                    break;
            }
        }

        // ------------------- 3️⃣ All Dies -------------------
        private bool CanNavigateAllDies() => _allWaferDies != null && _allWaferDies.Count >= 0;

        private void NavigateAllDies(AllDieNavigationType type)
        {
            if (!CanNavigateAllDies()) return;

            // ⭐ 선택된 게 없을 때 → 첫 번째로 자동 이동
            if (SelectedDie == null)
            {
                SelectedDie = _allWaferDies.FirstOrDefault();
                return;
            }

            int index = _allWaferDies.IndexOf(SelectedDie);
            if (index < 0) index = 0;

            switch (type)
            {
                case AllDieNavigationType.First:
                    SelectedDie = _allWaferDies.FirstOrDefault();
                    break;
                case AllDieNavigationType.Previous:
                    if (index > 0) SelectedDie = _allWaferDies[index - 1];
                    break;
                case AllDieNavigationType.Next:
                    if (index < _allWaferDies.Count - 1) SelectedDie = _allWaferDies[index + 1];
                    break;
                case AllDieNavigationType.Last:
                    SelectedDie = _allWaferDies.LastOrDefault();
                    break;
            }
        }

        // ------------------- UI 텍스트 갱신 -------------------
        private void UpdateDieNavInfo()
        {
            var list = Defects;
            var total = list?.Count ?? 0;

            if (total == 0)
            {
                CurrentDieInfo = "0 / 0";
                RaiseCanExecuteChanged();
                return;
            }

            if (SelectedDie == null)
            {
                CurrentDieInfo = $"- / {total}";
                RaiseCanExecuteChanged();
                return;
            }

            if (SelectedDie.IsGood)
            {
                CurrentDieInfo = "- / 0";
                RaiseCanExecuteChanged();
                return;
            }

            var currentItem = SelectedDefect;
            int currentIndex = (currentItem == null || list == null) ? -1 : list.IndexOf(currentItem);

            CurrentDieInfo = (currentIndex == -1) ? $"- / {total}" : $"{currentIndex + 1} / {total}";
            RaiseCanExecuteChanged();
        }

        private void UpdateDefectNavInfo()
        {
            var list = _allDefectiveDies;
            var total = list?.Count ?? 0;

            if (total == 0)
            {
                CurrentDefectInfo = "0 / 0";
                RaiseCanExecuteChanged();
                return;
            }

            var currentItem = SelectedDie;
            int currentIndex = (currentItem == null || list == null) ? -1 : list.IndexOf(currentItem);

            CurrentDefectInfo = (currentIndex == -1) ? $"- / {total}" : $"{currentIndex + 1} / {total}";
            RaiseCanExecuteChanged();
        }

        private void UpdateAllDieNavInfo()
        {
            var list = _allWaferDies;
            var total = list?.Count ?? 0;

            if (total == 0)
            {
                CurrentAllDieInfo = "0 / 0";
                RaiseCanExecuteChanged();
                return;
            }

            var currentItem = SelectedDie;
            int currentIndex = (currentItem == null || list == null) ? -1 : list.IndexOf(currentItem);

            CurrentAllDieInfo = (currentIndex == -1) ? $"- / {total}" : $"{currentIndex + 1} / {total}";
            RaiseCanExecuteChanged();
        }

        private void RaiseCanExecuteChanged()
        {
            (FirstDieCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PreviousDieCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NextDieCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (LastDieCommand as RelayCommand)?.RaiseCanExecuteChanged();

            (FirstDefectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PreviousDefectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NextDefectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (LastDefectCommand as RelayCommand)?.RaiseCanExecuteChanged();

            (FirstAllDieCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PreviousAllDieCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NextAllDieCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (LastAllDieCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
