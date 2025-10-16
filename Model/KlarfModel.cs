using System;
using System.Collections.ObjectModel;

namespace KlarfApplication.Model
{
    /// <summary>
    /// KLARF 파일의 전체 정보를 표현하는 모델 클래스
    /// </summary>
    public class KlarfModel : ModelBase
    {
        #region Fields

        private string _fileName;
        private string _filePath;
        private DateTime _fileDate;
        private string _fileExtension;
        private string _fileVersion;
        private DateTime _resultTimestamp;
        private string _inspectionStationId;
        private string _sampleType;
        private string _lotId;
        private string _waferId;
        private int _slot;
        private double _waferDiameter;
        private double _areaPerTest;
        private string _orientationMarkLocation;
        private string _tiffFileName;
        private double _diePitchX;
        private double _diePitchY;
        private bool _isParsed;

        #endregion

        #region Properties

        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(nameof(FileName)); }
        }

        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(nameof(FilePath)); }
        }

        public DateTime FileDate
        {
            get => _fileDate;
            set { _fileDate = value; OnPropertyChanged(nameof(FileDate)); }
        }

        public string FileExtension
        {
            get => _fileExtension;
            set { _fileExtension = value; OnPropertyChanged(nameof(FileExtension)); }
        }

        public string FileVersion
        {
            get => _fileVersion;
            set { _fileVersion = value; OnPropertyChanged(nameof(FileVersion)); }
        }

        public DateTime ResultTimestamp
        {
            get => _resultTimestamp;
            set { _resultTimestamp = value; OnPropertyChanged(nameof(ResultTimestamp)); }
        }

        public string InspectionStationId
        {
            get => _inspectionStationId;
            set { _inspectionStationId = value; OnPropertyChanged(nameof(InspectionStationId)); }
        }

        public string SampleType
        {
            get => _sampleType;
            set { _sampleType = value; OnPropertyChanged(nameof(SampleType)); }
        }

        public string LotId
        {
            get => _lotId;
            set { _lotId = value; OnPropertyChanged(nameof(LotId)); }
        }

        public string WaferId
        {
            get => _waferId;
            set { _waferId = value; OnPropertyChanged(nameof(WaferId)); }
        }

        public int Slot
        {
            get => _slot;
            set { _slot = value; OnPropertyChanged(nameof(Slot)); }
        }

        public double WaferDiameter
        {
            get => _waferDiameter;
            set { _waferDiameter = value; OnPropertyChanged(nameof(WaferDiameter)); }
        }

        public double AreaPerTest
        {
            get => _areaPerTest;
            set { _areaPerTest = value; OnPropertyChanged(nameof(AreaPerTest)); }
        }

        public string OrientationMarkLocation
        {
            get => _orientationMarkLocation;
            set { _orientationMarkLocation = value; OnPropertyChanged(nameof(OrientationMarkLocation)); }
        }

        public string TiffFileName
        {
            get => _tiffFileName;
            set { _tiffFileName = value; OnPropertyChanged(nameof(TiffFileName)); }
        }

        public double DiePitchX
        {
            get => _diePitchX;
            set { _diePitchX = value; OnPropertyChanged(nameof(DiePitchX)); }
        }

        public double DiePitchY
        {
            get => _diePitchY;
            set { _diePitchY = value; OnPropertyChanged(nameof(DiePitchY)); }
        }

        public ObservableCollection<DieModel> DieMap { get; set; } = new();
        public ObservableCollection<Defect> Defects { get; set; } = new();
        public ObservableCollection<string> DefectRecordSpec { get; set; } = new();

        public bool IsParsed
        {
            get => _isParsed;
            set { _isParsed = value; OnPropertyChanged(nameof(IsParsed)); }
        }

        public int TotalDefectCount => Defects.Count;
        public int TotalDies => DieMap.Count;

        #endregion
    }
}
