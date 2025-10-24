using KlarfApplication.Model;
using System; // ⭐️ [추가]
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace KlarfApplication.ViewModel
{
    public class DefectImageViewModel : ViewModelBase
    {
        // (DefectImage, NoImageVisibility 속성은 동일)
        private BitmapSource _defectImage;
        public BitmapSource DefectImage
        {
            get => _defectImage;
            set
            {
                _defectImage = value;
                OnPropertyChanged(nameof(DefectImage));
                OnPropertyChanged(nameof(NoImageVisibility));
            }
        }
        public Visibility NoImageVisibility
        {
            get
            {
                return (DefectImage == null) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #region Commands

        // (기존 ResetViewCommand, FitToWindowCommand)
        public ICommand ResetViewCommand { get; }
        public ICommand FitToWindowCommand { get; }

        // ⭐️ --- [추가 시작] ---
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }

        public event Action ZoomInRequested;
        public event Action ZoomOutRequested;
        // ⭐️ --- [추가 끝] ---


        public event Action ResetViewRequested;
        public event Action FitToWindowRequested;

        #endregion

        public DefectImageViewModel()
        {
            // (기존 Command 초기화)
            ResetViewCommand = new RelayCommand(() => ResetViewRequested?.Invoke());
            FitToWindowCommand = new RelayCommand(() => FitToWindowRequested?.Invoke());

            // ⭐️ --- [추가 시작] ---
            ZoomInCommand = new RelayCommand(() => ZoomInRequested?.Invoke());
            ZoomOutCommand = new RelayCommand(() => ZoomOutRequested?.Invoke());
            // ⭐️ --- [추가 끝] ---
        }

        // (LoadDefectImage, ClearImage, UpdateFromKlarf 메서드는 동일)
        public void LoadDefectImage(BitmapSource image)
        {
            DefectImage = image;
        }
        public void ClearImage()
        {
            DefectImage = null;
        }
        public void UpdateFromKlarf(KlarfModel klarf)
        {
            ClearImage();
        }
    }
}