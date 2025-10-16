using System;

namespace KlarfApplication.Model
{
    /// <summary>
    /// 결함(Defect) 정보를 나타내는 데이터 모델 클래스.
    /// KLARF의 DefectRecordSpec에 해당하며, 웨이퍼 내 좌표 및 분류 정보를 포함합니다.
    /// </summary>
    public class Defect : ModelBase
    {
        #region Fields

        private int _index;
        private string _defectId;
        private double _xCoord;
        private double _yCoord;
        private int _row;
        private int _column;
        private string _defectType;
        private double _size;
        private string _imagePath;
        private DateTime _detectedTime;

        #endregion

        #region Properties

        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                OnPropertyChanged(nameof(Index));
            }
        }

        public string DefectId
        {
            get => _defectId;
            set
            {
                _defectId = value;
                OnPropertyChanged(nameof(DefectId));
            }
        }

        public double XCoord
        {
            get => _xCoord;
            set
            {
                _xCoord = value;
                OnPropertyChanged(nameof(XCoord));
            }
        }

        public double YCoord
        {
            get => _yCoord;
            set
            {
                _yCoord = value;
                OnPropertyChanged(nameof(YCoord));
            }
        }

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

        public string DefectType
        {
            get => _defectType;
            set
            {
                _defectType = value;
                OnPropertyChanged(nameof(DefectType));
            }
        }

        public double Size
        {
            get => _size;
            set
            {
                _size = value;
                OnPropertyChanged(nameof(Size));
            }
        }

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        public DateTime DetectedTime
        {
            get => _detectedTime;
            set
            {
                _detectedTime = value;
                OnPropertyChanged(nameof(DetectedTime));
            }
        }

        #endregion

        #region Constructors

        public Defect() { }

        #endregion
    }
}
