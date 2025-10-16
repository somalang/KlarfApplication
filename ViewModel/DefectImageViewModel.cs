using KlarfApplication.Model;

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
