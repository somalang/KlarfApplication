using System.Collections.ObjectModel;

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

        /// <summary>
        /// 웨이퍼의 전체 지름(mm 단위).
        /// </summary>
        public double Diameter
        {
            get => _diameter;
            set
            {
                _diameter = value;
                OnPropertyChanged(nameof(Diameter));
            }
        }

        /// <summary>
        /// 웨이퍼 중심의 X 좌표.
        /// </summary>
        public double CenterX
        {
            get => _centerX;
            set
            {
                _centerX = value;
                OnPropertyChanged(nameof(CenterX));
            }
        }

        /// <summary>
        /// 웨이퍼 중심의 Y 좌표.
        /// </summary>
        public double CenterY
        {
            get => _centerY;
            set
            {
                _centerY = value;
                OnPropertyChanged(nameof(CenterY));
            }
        }

        /// <summary>
        /// 웨이퍼의 방향 정보 (예: Notch 위치, Flat 방향).
        /// </summary>
        public string Orientation
        {
            get => _orientation;
            set
            {
                _orientation = value;
                OnPropertyChanged(nameof(Orientation));
            }
        }

        /// <summary>
        /// 다이의 가로 크기(μm 단위).
        /// </summary>
        public double DieWidth
        {
            get => _dieWidth;
            set
            {
                _dieWidth = value;
                OnPropertyChanged(nameof(DieWidth));
            }
        }

        /// <summary>
        /// 다이의 세로 크기(μm 단위).
        /// </summary>
        public double DieHeight
        {
            get => _dieHeight;
            set
            {
                _dieHeight = value;
                OnPropertyChanged(nameof(DieHeight));
            }
        }

        /// <summary>
        /// 웨이퍼를 구성하는 다이 리스트.
        /// </summary>
        public ObservableCollection<DieModel> DiesList
        {
            get => _diesList;
            set
            {
                _diesList = value;
                OnPropertyChanged(nameof(DiesList));
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
