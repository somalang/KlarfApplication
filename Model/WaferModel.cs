using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KlarfApplication.Model
{
    internal class WaferModel : INotifyPropertyChanged
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
            get => _diameter;
            set { _diameter = value; OnPropertyChanged(nameof(Diameter)); }
        }

        public double CenterX
        {
            get => _centerX;
            set { _centerX = value; OnPropertyChanged(nameof(CenterX)); }
        }

        public double CenterY
        {
            get => _centerY;
            set { _centerY = value; OnPropertyChanged(nameof(CenterY)); }
        }

        public string Orientation
        {
            get => _orientation;
            set { _orientation = value; OnPropertyChanged(nameof(Orientation)); }
        }

        public double DieWidth
        {
            get => _dieWidth;
            set { _dieWidth = value; OnPropertyChanged(nameof(DieWidth)); }
        }

        public double DieHeight
        {
            get => _dieHeight;
            set { _dieHeight = value; OnPropertyChanged(nameof(DieHeight)); }
        }

        public ObservableCollection<DieModel> DiesList
        {
            get => _diesList;
            set { _diesList = value; OnPropertyChanged(nameof(DiesList)); }
        }

        public WaferModel()
        {
            DiesList = new ObservableCollection<DieModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
