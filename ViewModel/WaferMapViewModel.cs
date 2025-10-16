using KlarfApplication.Model;
using System.Collections.ObjectModel;

namespace KlarfApplication.ViewModel
{
    public class WaferViewModel : ViewModelBase
    {
        private WaferModel _wafer;
        public WaferModel Wafer
        {
            get => _wafer;
            set
            {
                _wafer = value;
                OnPropertyChanged(nameof(Wafer));
            }
        }

        public void UpdateFromKlarf(KlarfModel klarf)
        {
            if (klarf == null) return;

            Wafer = new WaferModel
            {
                Diameter = klarf.WaferDiameter,
                DieWidth = klarf.DiePitchX,
                DieHeight = klarf.DiePitchY,
                Orientation = klarf.OrientationMarkLocation
            };

            // KlarfModel의 DieMap을 WaferModel로 매핑
            foreach (var die in klarf.DieMap)
            {
                Wafer.DiesList.Add(die);
            }
        }
    }
}
