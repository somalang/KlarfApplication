using System.Collections.ObjectModel;

namespace KlarfApplication.Model
{
    /// <summary>
    /// 다이(Die) 정보를 나타내는 데이터 모델 클래스.
    /// 웨이퍼 내 위치(Row, Column), 불량 여부 및 포함된 결함 리스트를 포함합니다.
    /// </summary>
    public class DieModel : ModelBase
    {
        #region Fields

        private int _row;
        private int _column;
        private double _centerX;
        private double _centerY;
        private bool _isGood;
        private bool _isSelected;
        private string _orientation;
        private ObservableCollection<Defect> _defectsList;

        #endregion

        #region Properties

        public int Row
        {
            get => _row;
            set
            {
                _row = value;
                OnPropertyChanged(nameof(Row));
            }
        }

        public int Column
        {
            get => _column;
            set
            {
                _column = value;
                OnPropertyChanged(nameof(Column));
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

        public bool IsGood
        {
            get => _isGood;
            set
            {
                _isGood = value;
                OnPropertyChanged(nameof(IsGood));
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
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

        public ObservableCollection<Defect> DefectsList
        {
            get => _defectsList;
            set
            {
                _defectsList = value;
                OnPropertyChanged(nameof(DefectsList));
            }
        }

        #endregion

        #region Constructors

        public DieModel()
        {
            DefectsList = new ObservableCollection<Defect>();
        }

        #endregion
    }
}
