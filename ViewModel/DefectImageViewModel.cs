using KlarfApplication.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging; // ⭐️ [추가] BitmapSource 사용

namespace KlarfApplication.ViewModel
{
    public class DefectImageViewModel : ViewModelBase
    {
        // ⭐️ [수정] string ImagePath 대신 BitmapSource DefectImage 사용
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

        // ⭐️ [삭제] Defects 리스트는 이 VM에서 더 이상 필요 없음
        // (원본 파일의 private ObservableCollection<Defect> _defects; ... 부분 삭제)

        public Visibility NoImageVisibility
        {
            get
            {
                // ⭐️ [수정] DefectImage가 null일 때 "No Image" 표시
                return (DefectImage == null) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// ⭐️ [추가] MainViewModel이 이 메서드를 호출하여 이미지를 로드합니다.
        /// </summary>
        public void LoadDefectImage(BitmapSource image)
        {
            DefectImage = image;
        }

        /// <summary>
        /// ⭐️ [추가] MainViewModel이 이 메서드를 호출하여 이미지를 지웁니다.
        /// </summary>
        public void ClearImage()
        {
            DefectImage = null;
        }

        /// <summary>
        /// ⭐️ [수정] Klarf 파일이 변경되면 일단 이미지를 지웁니다.
        /// (원본 파일의 UpdateFromKlarf 로직을 아래 한 줄로 대체)
        /// </summary>
        public void UpdateFromKlarf(KlarfModel klarf)
        {
            ClearImage();
        }

    }
}