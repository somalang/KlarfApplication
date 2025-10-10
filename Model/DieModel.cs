using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KlarfApplication.Model
{
    internal class DieModel : INotifyPropertyChanged
    {
        // die 위치, 불량 여부, 불량 리스트, 중심 좌표, 선택 여부, 회전 방향
        private int _row;
        private int _column;
        private double _centerX;
        private double _centerY;
        private bool _isGood;
        private bool _isSelected;
        private string _orientation; // 추가됨 (회전 방향)
        private ObservableCollection<Defect> _defectsList; // ← DefectModel이라면 이름 맞게 수정

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

        public bool IsGood
        {
            get => _isGood;
            set { _isGood = value; OnPropertyChanged(nameof(IsGood)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public string Orientation
        {
            get => _orientation;
            set { _orientation = value; OnPropertyChanged(nameof(Orientation)); }
        }

        public ObservableCollection<Defect> DefectsList
        {
            get => _defectsList;
            set { _defectsList = value; OnPropertyChanged(nameof(DefectsList)); }
        }

        public DieModel()
        {
            DefectsList = new ObservableCollection<Defect>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
