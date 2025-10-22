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
        private string _defectType; // [유지] CLASSNUMBER
        private double _size;       // [유지] DSIZE
        private string _imagePath;
        private DateTime _detectedTime;

        // [추가] 새로운 DefectRecordSpec 17 필드
        private double _xSize;
        private double _ySize;
        private double _defectArea;
        private int _test;
        private int _clusterNumber;
        private int _roughBinNumber;
        private int _fineBinNumber;
        private int _reviewSample;
        private int _imageCount;
        private string _imageList;

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

        // CLASSNUMBER
        public string DefectType
        {
            get => _defectType;
            set
            {
                _defectType = value;
                OnPropertyChanged(nameof(DefectType));
            }
        }

        // DSIZE
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

        // [추가] 새로운 Defect 속성
        public double XSize
        {
            get => _xSize;
            set { _xSize = value; OnPropertyChanged(nameof(XSize)); }
        }

        public double YSize
        {
            get => _ySize;
            set { _ySize = value; OnPropertyChanged(nameof(YSize)); }
        }

        public double DefectArea
        {
            get => _defectArea;
            set { _defectArea = value; OnPropertyChanged(nameof(DefectArea)); }
        }

        public int Test
        {
            get => _test;
            set { _test = value; OnPropertyChanged(nameof(Test)); }
        }

        public int ClusterNumber
        {
            get => _clusterNumber;
            set { _clusterNumber = value; OnPropertyChanged(nameof(ClusterNumber)); }
        }

        public int RoughBinNumber
        {
            get => _roughBinNumber;
            set { _roughBinNumber = value; OnPropertyChanged(nameof(RoughBinNumber)); }
        }

        public int FineBinNumber
        {
            get => _fineBinNumber;
            set { _fineBinNumber = value; OnPropertyChanged(nameof(FineBinNumber)); }
        }

        public int ReviewSample
        {
            get => _reviewSample;
            set { _reviewSample = value; OnPropertyChanged(nameof(ReviewSample)); }
        }

        public int ImageCount
        {
            get => _imageCount;
            set { _imageCount = value; OnPropertyChanged(nameof(ImageCount)); }
        }

        public string ImageList
        {
            get => _imageList;
            set { _imageList = value; OnPropertyChanged(nameof(ImageList)); }
        }


        #endregion

        #region Constructors

        public Defect() { }

        #endregion
    }
}