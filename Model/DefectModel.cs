using System;
using System.ComponentModel;

namespace KlarfApplication.Model
{
    internal class Defect : INotifyPropertyChanged
    {
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

        public int Index
        {
            get => _index;
            set { _index = value; OnPropertyChanged(nameof(Index)); }
        }

        public string DefectId
        {
            get => _defectId;
            set { _defectId = value; OnPropertyChanged(nameof(DefectId)); }
        }

        public double XCoord
        {
            get => _xCoord;
            set { _xCoord = value; OnPropertyChanged(nameof(XCoord)); }
        }

        public double YCoord
        {
            get => _yCoord;
            set { _yCoord = value; OnPropertyChanged(nameof(YCoord)); }
        }

        public int Row
        {
            get => _row;
            set { _row = value; OnPropertyChanged(nameof(Row)); }
        }

        public int Column
        {
            get => _column;
            set { _column = value; OnPropertyChanged(nameof(Column)); }
        }

        public string DefectType
        {
            get => _defectType;
            set { _defectType = value; OnPropertyChanged(nameof(DefectType)); }
        }

        public double Size
        {
            get => _size;
            set { _size = value; OnPropertyChanged(nameof(Size)); }
        }

        public string ImagePath
        {
            get => _imagePath;
            set { _imagePath = value; OnPropertyChanged(nameof(ImagePath)); }
        }

        public DateTime DetectedTime
        {
            get => _detectedTime;
            set { _detectedTime = value; OnPropertyChanged(nameof(DetectedTime)); }
        }

        public Defect() { }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
