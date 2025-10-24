using KlarfApplication.Model;
using System;
using System.Collections.Generic; // ⭐️ [추가] List 사용
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input; // ⭐️ [추가] ICommand 사용
using KlarfApplication; // ⭐️ [추가] RelayCommand 사용을 위해

namespace KlarfApplication.ViewModel
{
    public class WaferViewModel : ViewModelBase
    {
        private WaferModel _wafer;
        private ObservableCollection<DieViewModel> _dies;
        private string _waferMapStats;
        private Visibility _noImageVisibility = Visibility.Visible;
        private DieViewModel _selectedDie;

        // ⭐️ [추가] 전체 Defect 리스트
        private List<Defect> _allDefects;

        public WaferModel Wafer
        {
            get => _wafer;
            set { _wafer = value; OnPropertyChanged(nameof(Wafer)); }
        }

        public ObservableCollection<DieViewModel> Dies
        {
            get => _dies;
            set { _dies = value; OnPropertyChanged(nameof(Dies)); }
        }

        // ⭐️ [추가] View에서 사용할 전체 Defect 리스트 (읽기 전용)
        public IEnumerable<Defect> AllDefects => _allDefects;

        public string WaferMapStats
        {
            get => _waferMapStats;
            set { _waferMapStats = value; OnPropertyChanged(nameof(WaferMapStats)); }
        }

        public Visibility NoImageVisibility
        {
            get => _noImageVisibility;
            set { _noImageVisibility = value; OnPropertyChanged(nameof(NoImageVisibility)); }
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
                }
            }
        }

        // ⭐️ --- [줌 Command 및 이벤트 추가 시작] ---
        #region Commands
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand ResetViewCommand { get; }
        public ICommand FitToWindowCommand { get; } // FitToWindow는 Reset과 동일하게 동작

        public event Action ZoomInRequested;
        public event Action ZoomOutRequested;
        public event Action ResetViewRequested;
        #endregion

        public WaferViewModel() // 생성자
        {
            _allDefects = new List<Defect>(); // 초기화

            // Command 초기화
            ZoomInCommand = new RelayCommand(() => ZoomInRequested?.Invoke());
            ZoomOutCommand = new RelayCommand(() => ZoomOutRequested?.Invoke());
            ResetViewCommand = new RelayCommand(() => ResetViewRequested?.Invoke());
            FitToWindowCommand = new RelayCommand(() => ResetViewRequested?.Invoke()); // Fit도 Reset 호출
        }
        // ⭐️ --- [줌 Command 및 이벤트 추가 끝] ---


        public void UpdateFromKlarf(KlarfModel klarf)
        {
            if (klarf == null || !klarf.IsParsed)
            {
                Dies = new ObservableCollection<DieViewModel>();
                _allDefects = new List<Defect>(); // ⭐️ Defect 리스트 초기화
                NoImageVisibility = Visibility.Visible;
                WaferMapStats = "";
                Wafer = null; // ⭐️ Wafer 정보도 초기화
                OnPropertyChanged(nameof(AllDefects)); // ⭐️ View에 변경 알림
                return;
            }

            // ⭐️ Defect 리스트 저장
            _allDefects = klarf.Defects.ToList();
            OnPropertyChanged(nameof(AllDefects)); // ⭐️ View에 변경 알림

            Wafer = new WaferModel
            {
                Diameter = klarf.WaferDiameter,
                DieWidth = klarf.DiePitchX, // Die의 실제 너비 (좌표 계산용)
                DieHeight = klarf.DiePitchY, // Die의 실제 높이 (좌표 계산용)
                Orientation = klarf.OrientationMarkLocation
            };

            // ⭐️ DieMap이 비어있을 경우 예외 처리
            if (klarf.DieMap == null || !klarf.DieMap.Any())
            {
                Dies = new ObservableCollection<DieViewModel>();
                NoImageVisibility = Visibility.Visible;
                WaferMapStats = "No Die data in Klarf";
                return;
            }

            // Die 좌표 범위 계산
            var minRow = klarf.DieMap.Min(d => d.Row);
            var maxRow = klarf.DieMap.Max(d => d.Row);
            var minCol = klarf.DieMap.Min(d => d.Column);
            var maxCol = klarf.DieMap.Max(d => d.Column);

            var dieViewModels = new ObservableCollection<DieViewModel>();

            foreach (var die in klarf.DieMap)
            {
                // 해당 Die에 속한 Defect 개수 계산 (IsGood 판별용)
                var defectCount = _allDefects.Count(d => d.Row == die.Row && d.Column == die.Column);

                dieViewModels.Add(new DieViewModel
                {
                    Row = die.Row, // Die 인덱스 (X)
                    Column = die.Column, // Die 인덱스 (Y)
                    // CenterX, CenterY, Width, Height는 이제 View에서 계산하므로 ViewModel에서는 불필요
                    IsGood = defectCount == 0,
                    IsEdge = false, // (가장자리 판별 로직은 아직 없음)
                    DefectCount = defectCount // 정보 표시용
                });
            }

            Dies = dieViewModels;

            // 통계 계산
            int totalDies = Dies.Count;
            int goodDies = Dies.Count(d => !d.IsEdge && d.IsGood);
            int defectiveDies = Dies.Count(d => !d.IsEdge && !d.IsGood);
            int edgeDies = 0;
            double yield = totalDies - edgeDies > 0 ? (double)goodDies / (totalDies - edgeDies) * 100 : 0;

            WaferMapStats = $"Total: {totalDies} | Good: {goodDies} | Defect: {defectiveDies} | Edge: {edgeDies} | Yield: {yield:F1}%";
            NoImageVisibility = Visibility.Collapsed;

            // ⭐️ 데이터 로드 후 View 리셋 요청 (선택 사항)
            // ResetViewRequested?.Invoke();
        }
    }

    // DieViewModel 구조 단순화 (View에서 좌표 계산)
    public class DieViewModel : ViewModelBase
    {
        public int Row { get; set; }
        public int Column { get; set; }
        // CenterX, CenterY, Width, Height 제거
        public bool IsGood { get; set; }
        public bool IsEdge { get; set; }
        public int DefectCount { get; set; }

        public string DieType => IsGood ? "Good" : "Defective";

        // ⭐️ Die 식별을 위한 키 추가 (View에서 Dictionary Key로 사용)
        public string Key => $"{Row},{Column}";
    }
}
