using KlarfApplication.Model;
using System.Collections.ObjectModel;
using System.Windows;

namespace KlarfApplication.ViewModel
{
    public class DefectInfoViewModel : ViewModelBase
    {
        private ObservableCollection<Defect> _defects;
        private DieViewModel _selectedDie;
        private KlarfModel _currentKlarfFile;
        public ObservableCollection<Defect> Defects
        {
            get => _defects;
            set
            {
                _defects = value;
                OnPropertyChanged(nameof(Defects));
                OnPropertyChanged(nameof(NoDefectsVisibility));
                OnPropertyChanged(nameof(DefectCount));
            }
        }
        public Visibility NoDefectsVisibility
        {
            get
            {
                return (Defects == null || Defects.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
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
                    OnPropertyChanged(nameof(SelectedDieVisibility)); // 👈 표시 여부도 함께 갱신
                }
            }
        }
        public Visibility SelectedDieVisibility
        {
            get
            {
                return _selectedDie == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        public KlarfModel CurrentKlarfFile
        {
            get => _currentKlarfFile;
            private set // 외부에서는 수정 불가
            {
                _currentKlarfFile = value;
                OnPropertyChanged(nameof(CurrentKlarfFile));
                // ⭐️ CurrentKlarfFile이 변경될 때 DefectCount도 갱신 알림 (null이 될 수 있으므로)
                OnPropertyChanged(nameof(DefectCount));
            }
        }

        // ⭐️ [추가] 헤더에 표시할 Defect 개수 (Null 처리 포함)
        public int DefectCount => CurrentKlarfFile?.TotalDefectCount ?? 0;
        public void UpdateFromKlarf(KlarfModel klarf)
        {
            CurrentKlarfFile = klarf;
            if (klarf == null)
            {
                Defects = new ObservableCollection<Defect>();
                SelectedDie = null;
                return;
            }

            Defects = new ObservableCollection<Defect>(klarf.Defects);
        }
    }
}
