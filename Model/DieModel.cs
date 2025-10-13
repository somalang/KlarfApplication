using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using KlarfApplication.Model;

namespace KlarfApplication.Model
{
    public class DieModel : ModelBase
    {
        // die 위치, 불량 여부, 불량 리스트, 중심 좌표, 선택 여부, 회전 방향
        private int _row;
        private int _column;
        private double _centerX;
        private double _centerY;
        private bool _isGood;
        private bool _isSelected;
        private string _orientation; 
        private ObservableCollection<Defect> _defectsList; 

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

        public bool IsGood
        {
            get
            {
                return _isGood;
            }
            set
            {
                _isGood = value;
                OnPropertyChanged(nameof(IsGood));
            }
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set { 
                _isSelected = value; 
                OnPropertyChanged(nameof(IsSelected)); 
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

        public ObservableCollection<Defect> DefectsList
        {
            get
            {
                return _defectsList;
            }
            set { 
                _defectsList = value; 
                OnPropertyChanged(nameof(DefectsList)); 
            }
        }

        public DieModel()
        {
            DefectsList = new ObservableCollection<Defect>();
        }

    }
}
