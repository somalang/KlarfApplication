using KlarfApplication.Model;
using System.Collections.ObjectModel;
using System.Linq; // ⭐️ [추가] .Where(), .FirstOrDefault() 사용
using System.Windows;

namespace KlarfApplication.ViewModel
{
    public class DefectInfoViewModel : ViewModelBase
    {
        private KlarfModel _currentKlarfFile;
        private DieViewModel _selectedDie;
        private Defect _selectedDefect; // ⭐️ [추가] 'SelectedDefect' 오류 해결
        private ObservableCollection<Defect> _allDefects; // ⭐️ [추가] 원본 (필터링되지 않은) Defect 리스트
        private ObservableCollection<Defect> _defects; // 뷰에 바인딩된 (필터링된) Defect 리스트

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
                    FilterDefectsBySelectedDie(); // ⭐️ [추가] Die 선택 시 Defect 리스트 필터링
                }
            }
        }

        /// <summary>
        /// ⭐️ [추가] DataGrid에서 선택된 현재 Defect (MainViewModel에서 사용)
        /// </summary>
        public Defect SelectedDefect
        {
            get => _selectedDefect;
            set
            {
                _selectedDefect = value;
                OnPropertyChanged(nameof(SelectedDefect));
                // 이 변경을 MainViewModel이 감지합니다.
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
                return;
            }

            // ⭐️ [수정] 원본 리스트와 뷰 리스트를 모두 초기화
            _allDefects = new ObservableCollection<Defect>(klarf.Defects);
            Defects = new ObservableCollection<Defect>(_allDefects);
        }

        /// <summary>
        /// ⭐️ [추가] 선택된 Die에 해당하는 Defect만 표시하도록 Defects 리스트를 필터링합니다.
        /// </summary>
        private void FilterDefectsBySelectedDie()
        {
            if (_selectedDie == null)
            {
                // 선택된 Die가 없으면 전체 Defect 표시
                Defects = new ObservableCollection<Defect>(_allDefects ?? new ObservableCollection<Defect>());
            }
            else
            {
                // 선택된 Die의 Row/Column과 일치하는 Defect만 필터링
                var filtered = _allDefects.Where(d =>
                    d.Row == _selectedDie.Row &&
                    d.Column == _selectedDie.Column);
                Defects = new ObservableCollection<Defect>(filtered);
            }

            // ⭐️ [추가] 필터링 후, 리스트의 첫 번째 Defect를 자동으로 선택
            SelectedDefect = Defects.FirstOrDefault();
        }
    }
}