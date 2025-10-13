using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KlarfApplication.Model
{
    internal class WaferModel : ModelBase
    {
        // 웨이퍼 지름, 중심 x,y 좌표, 불량률, die 크기, die 리스트, 방향
        private double _diameter;
        private double _centerX;
        private double _centerY;
        private string _orientation;
        private double _dieWidth;
        private double _dieHeight;
        private ObservableCollection<DieModel> _diesList;

        public double Diameter
        {
            get
            {
                return _diameter;
            }
            set { 
                _diameter = value; 
                OnPropertyChanged(nameof(Diameter)); 
            }
        }

        public double CenterX
        {
            get
            {
                return _centerX;
            }
            set { 
                _centerX = value; 
                OnPropertyChanged(nameof(CenterX));
            }
        }

        public double CenterY
        {
            get
            {
                return _centerY;
            }
            set { 
                _centerY = value; 
                OnPropertyChanged(nameof(CenterY)); 
            }
        }

        public string Orientation
        {
            get
            {
                return _orientation;
            }
            set { 
                _orientation = value; 
                OnPropertyChanged(nameof(Orientation)); 
            }
        }

        public double DieWidth
        {
            get
            {
                return _dieWidth;
            }
            set { 
                _dieWidth = value; 
                OnPropertyChanged(nameof(DieWidth)); 
            }
        }

        public double DieHeight
        {
            get
            {
                return _dieHeight;
            }
            set { 
                _dieHeight = value; 
                OnPropertyChanged(nameof(DieHeight)); 
            }
        }

        public ObservableCollection<DieModel> DiesList
        {
            get
            {
                return _diesList;
            }
            set { 
                _diesList = value; 
                OnPropertyChanged(nameof(DiesList));
            }
        }

        public WaferModel()
        {
            DiesList = new ObservableCollection<DieModel>();
        }
    }
}
