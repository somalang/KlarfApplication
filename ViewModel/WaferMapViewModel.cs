// ViewModel/WaferMapViewModel.cs
using KlarfApplication.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace KlarfApplication.ViewModel
{
    public class WaferViewModel : ViewModelBase
    {
        private WaferModel _wafer;
        private ObservableCollection<DieViewModel> _dies;

        public WaferModel Wafer
        {
            get => _wafer;
            set
            {
                _wafer = value;
                OnPropertyChanged(nameof(Wafer));
            }
        }

        public ObservableCollection<DieViewModel> Dies
        {
            get => _dies;
            set
            {
                _dies = value;
                OnPropertyChanged(nameof(Dies));
            }
        }

        public void UpdateFromKlarf(KlarfModel klarf)
        {
            if (klarf == null || !klarf.IsParsed)
            {
                Dies = new ObservableCollection<DieViewModel>();
                return;
            }

            Wafer = new WaferModel
            {
                Diameter = klarf.WaferDiameter,
                DieWidth = klarf.DiePitchX,
                DieHeight = klarf.DiePitchY,
                Orientation = klarf.OrientationMarkLocation
            };

            // Die 데이터를 ViewModel로 변환
            var dieViewModels = new ObservableCollection<DieViewModel>();

            foreach (var die in klarf.DieMap)
            {
                // 해당 Die에 속한 Defect 개수 계산
                var defectCount = klarf.Defects.Count(d => d.Row == die.Row && d.Column == die.Column);

                dieViewModels.Add(new DieViewModel
                {
                    Row = die.Row,
                    Column = die.Column,
                    CenterX = die.Row * klarf.DiePitchX,
                    CenterY = die.Column * klarf.DiePitchY,
                    Width = klarf.DiePitchX,
                    Height = klarf.DiePitchY,
                    IsGood = defectCount == 0,
                    DefectCount = defectCount
                });
            }

            Dies = dieViewModels;
        }
    }

    // Die를 화면에 그리기 위한 ViewModel
    public class DieViewModel : ViewModelBase
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsGood { get; set; }
        public int DefectCount { get; set; }
    }
}