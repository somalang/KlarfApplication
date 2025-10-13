using System;
using System.ComponentModel;

namespace KlarfApplication.Model
{
    public class Defect : ModelBase
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
            get
            {
                return _index;
            }
            set { 
                _index = value; 
                OnPropertyChanged(nameof(Index)); 
            }
        }

        public string DefectId
        {
            get
            {
                return _defectId;
            }
            set { 
                _defectId = value; 
                OnPropertyChanged(nameof(DefectId)); 
            }
        }

        public double XCoord
        {
            get
            {
                return _xCoord;
            }
            set { 
                _xCoord = value; 
                OnPropertyChanged(nameof(XCoord)); 
            }
        }

        public double YCoord
        {
            get
            {
                return _yCoord;
            }
            set { 
                _yCoord = value; 
                OnPropertyChanged(nameof(YCoord)); 
            }
        }

        public int Row
        {
            get
            {
                return _row;
            }
            set { 
                _row = value; 
                OnPropertyChanged(nameof(Row)); 
            }
        }

        public int Column
        {
            get
            {
                return _column;
            }
            set {
                _column = value; 
                OnPropertyChanged(nameof(Column)); 
            }
        }

        public string DefectType
        {
            get
            {
                return _defectType;
            }
            set { 
                _defectType = value; 
                OnPropertyChanged(nameof(DefectType)); 
            }
        }

        public double Size
        {
            get
            {
                return _size;
            }
            set { 
                _size = value; 
                OnPropertyChanged(nameof(Size)); 
            }
        }

        public string ImagePath
        {
            get
            {
               return _imagePath;
            }
            set { 
                _imagePath = value; 
                OnPropertyChanged(nameof(ImagePath)); 
            }
        }

        public DateTime DetectedTime
        {
            get
            {
                return _detectedTime;
            }
            set { 
                _detectedTime = value; 
                OnPropertyChanged(nameof(DetectedTime)); 
            }
        }

        public Defect() { }            
    }
}
