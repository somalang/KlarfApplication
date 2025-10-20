using KlarfApplication.Model;
using System.Collections.ObjectModel;
using System.Windows;

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
                OnPropertyChanged(nameof(NoDefectsVisibility));
            }
        }
        public Visibility NoDefectsVisibility
        {
            get
            {
                return (Defects == null || Defects.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
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
