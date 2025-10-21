using KlarfApplication.Model;
using System.Collections.ObjectModel;
using System.Windows;

namespace KlarfApplication.ViewModel
{
    public class DefectImageViewModel : ViewModelBase
    {
        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged(nameof(ImagePath));
            }
        }
        private ObservableCollection<Defect> _defects;
        public ObservableCollection<Defect> Defects
        {
            get => _defects;
            set
            {
                _defects = value;
                OnPropertyChanged(nameof(Defects));
                OnPropertyChanged(nameof(NoImageVisibility));
            }
        }
        public Visibility NoImageVisibility
        {
            get
            {
                return (Defects == null || Defects.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public void UpdateFromKlarf(KlarfModel klarf)
        {
            if (klarf == null || klarf.Defects.Count == 0)
            {
                ImagePath = string.Empty;
                return;
            }

            ImagePath = klarf.Defects.FirstOrDefault()?.ImagePath ?? string.Empty;
        }

    }
}
