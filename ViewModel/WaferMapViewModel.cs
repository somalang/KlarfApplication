using KlarfApplication.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace KlarfApplication.ViewModel
{
    public class WaferViewModel : ViewModelBase
    {
        private WaferModel _wafer;
        private ObservableCollection<DieViewModel> _dies;
        private string _waferMapStats;
        private Visibility _noImageVisibility = Visibility.Visible;

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

        public string WaferMapStats
        {
            get => _waferMapStats;
            set
            {
                _waferMapStats = value;
                OnPropertyChanged(nameof(WaferMapStats));
            }
        }

        public Visibility NoImageVisibility
        {
            get => _noImageVisibility;
            set
            {
                _noImageVisibility = value;
                OnPropertyChanged(nameof(NoImageVisibility));
            }
        }

        public void UpdateFromKlarf(KlarfModel klarf)
        {
            if (klarf == null || !klarf.IsParsed)
            {
                Dies = new ObservableCollection<DieViewModel>();
                NoImageVisibility = Visibility.Visible;
                WaferMapStats = "";
                return;
            }

            Wafer = new WaferModel
            {
                Diameter = klarf.WaferDiameter,
                DieWidth = klarf.DiePitchX,
                DieHeight = klarf.DiePitchY,
                Orientation = klarf.OrientationMarkLocation
            };

            // Die 좌표 범위 계산
            var minX = klarf.DieMap.Min(d => d.Row);
            var maxX = klarf.DieMap.Max(d => d.Row);
            var minY = klarf.DieMap.Min(d => d.Column);
            var maxY = klarf.DieMap.Max(d => d.Column);

            double centerX = (minX + maxX) / 2.0;
            double centerY = (minY + maxY) / 2.0;
            double waferRadiusDies = Math.Max(maxX - minX, maxY - minY) / 2.0;

            var dieViewModels = new ObservableCollection<DieViewModel>();

            foreach (var die in klarf.DieMap)
            {
                // 웨이퍼 중심으로부터의 거리 계산
                double distanceX = die.Row - centerX;
                double distanceY = die.Column - centerY;
                double distanceFromCenter = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

                // Edge die 판별
                bool isEdge = distanceFromCenter >= waferRadiusDies * 0.85;

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
                    IsEdge = isEdge,
                    DefectCount = defectCount
                });
            }

            Dies = dieViewModels;

            // 통계 계산
            int totalDies = Dies.Count;
            int goodDies = Dies.Count(d => !d.IsEdge && d.IsGood);
            int defectiveDies = Dies.Count(d => !d.IsEdge && !d.IsGood);
            int edgeDies = Dies.Count(d => d.IsEdge);
            double yield = totalDies - edgeDies > 0 ? (double)goodDies / (totalDies - edgeDies) * 100 : 0;

            WaferMapStats = $"Total: {totalDies} | Good: {goodDies} | Defect: {defectiveDies} | Edge: {edgeDies} | Yield: {yield:F1}%";
            NoImageVisibility = Visibility.Collapsed;
        }
    }

    public class DieViewModel : ViewModelBase
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsGood { get; set; }
        public bool IsEdge { get; set; }
        public int DefectCount { get; set; }

        public string DieType
        {
            get
            {
                if (IsEdge) return "Edge";
                if (IsGood) return "Good";
                return "Defective";
            }
        }
    }
}