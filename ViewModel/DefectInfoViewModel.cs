using KlarfApplication.Model;
using System.Collections.ObjectModel;

namespace KlarfApplication.ViewModel
{
    public class DefectInfoViewModel : ViewModelBase
    {
        private ObservableCollection<Defect> _defects;
        public ObservableCollection<Defect> Defects
        {
            get => _defects;
            set
            {
                _defects = value;
                OnPropertyChanged(nameof(Defects));
            }
        }

        public void UpdateFromKlarf(KlarfModel klarf)
        {
            if (klarf == null)
            {
                Defects = new ObservableCollection<Defect>();
                return;
            }

            Defects = new ObservableCollection<Defect>(klarf.Defects);
        }
    }
}
