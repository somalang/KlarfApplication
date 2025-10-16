using System.Collections.ObjectModel;
using System.Linq;

namespace KlarfApplication.Model
{
    /// <summary>
    /// 웨이퍼(Wafer) 정보를 나타내는 데이터 모델 클래스.
    /// KLARF 파일 내의 웨이퍼 지름, 중심 좌표, 다이 크기 및 다이 리스트를 관리합니다.
    /// </summary>
    public class WaferModel : ModelBase
    {
        #region Fields

        private double _diameter;
        private double _centerX;
        private double _centerY;
        private string _orientation;
        private double _dieWidth;
        private double _dieHeight;
        private ObservableCollection<DieModel> _diesList;

        #endregion

        #region Properties

        public double Diameter
        {
            get => _diameter;
            set
            {
                _diameter = value;
                OnPropertyChanged(nameof(Diameter));
            }
        }

        public double CenterX
        {
            get => _centerX;
            set
            {
                _centerX = value;
                OnPropertyChanged(nameof(CenterX));
            }
        }

        public double CenterY
        {
            get => _centerY;
            set
            {
                _centerY = value;
                OnPropertyChanged(nameof(CenterY));
            }
        }

        public string Orientation
        {
            get => _orientation;
            set
            {
                _orientation = value;
                OnPropertyChanged(nameof(Orientation));
            }
        }

        public double DieWidth
        {
            get => _dieWidth;
            set
            {
                _dieWidth = value;
                OnPropertyChanged(nameof(DieWidth));
            }
        }

        public double DieHeight
        {
            get => _dieHeight;
            set
            {
                _dieHeight = value;
                OnPropertyChanged(nameof(DieHeight));
            }
        }

        public ObservableCollection<DieModel> DiesList
        {
            get => _diesList;
            set
            {
                _diesList = value;
                OnPropertyChanged(nameof(DiesList));
            }
        }

        /// <summary>
        /// 전체 다이 중 불량 다이 비율(%)을 반환합니다.
        /// </summary>
        public double DefectRate
        {
            get
            {
                if (DiesList == null || DiesList.Count == 0)
                    return 0.0;

                int defectDies = DiesList.Count(die => !die.IsGood);
                return (double)defectDies / DiesList.Count * 100.0;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 기본 생성자. 다이 리스트를 초기화합니다.
        /// </summary>
        public WaferModel()
        {
            DiesList = new ObservableCollection<DieModel>();
        }

        #endregion
    }
}
