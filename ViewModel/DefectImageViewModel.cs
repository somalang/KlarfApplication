using KlarfApplication.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using KlarfApplication.Service; // ⭐️ [추가] ImageService 사용

namespace KlarfApplication.ViewModel
{
    public class DefectImageViewModel : ViewModelBase
    {
        // ⭐️ [수정] 원본 이미지와 표시용 이미지를 분리
        private BitmapSource _originalDefectImage; // 원본 (효과 미적용)
        private BitmapSource _displayDefectImage;  // 표시용 (효과 적용)
        private readonly ImageService _imageService; // ⭐️ [추가]

        public BitmapSource DefectImage
        {
            get => _displayDefectImage; // ⭐️ 표시용 이미지를 반환
            set
            {
                _displayDefectImage = value;
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

        // ⭐️ --- [밝기/대비 프로퍼티 추가 시작] ---
        private double _brightness = 0;
        public double Brightness
        {
            get => _brightness;
            set
            {
                if (_brightness != value)
                {
                    _brightness = value;
                    OnPropertyChanged(nameof(Brightness));
                    UpdateImageEffects(); // ⭐️ 값이 바뀌면 효과 갱신
                }
            }
        }

        private double _contrast = 0;
        public double Contrast
        {
            get => _contrast;
            set
            {
                if (_contrast != value)
                {
                    _contrast = value;
                    OnPropertyChanged(nameof(Contrast));
                    UpdateImageEffects(); // ⭐️ 값이 바뀌면 효과 갱신
                }
            }
        }
        // ⭐️ --- [밝기/대비 프로퍼티 추가 끝] ---


        public DefectImageViewModel()
        {
            _imageService = new ImageService(); // ⭐️ [추가] 서비스 인스턴스 생성

            // ⭐️ [수정] ResetViewCommand가 ResetView 메서드를 호출하도록 변경
            ResetViewCommand = new RelayCommand(ResetView);
            FitToWindowCommand = new RelayCommand(() => FitToWindowRequested?.Invoke());

            // ⭐️ --- [추가 시작] ---
            ZoomInCommand = new RelayCommand(() => ZoomInRequested?.Invoke());
            ZoomOutCommand = new RelayCommand(() => ZoomOutRequested?.Invoke());
            // ⭐️ --- [추가 끝] ---
        }
        private double _pixelScale;
        public double PixelScale
        {
            get => _pixelScale;
            set
            {
                _pixelScale = value;
                OnPropertyChanged(nameof(PixelScale));
            }
        }

        public void LoadDefectImage(BitmapSource image, double pixelScale)
        {
            _originalDefectImage = image; // ⭐️ 원본 이미지 저장
            PixelScale = pixelScale;

            // ⭐️ 이미지 로드 시 밝기/대비 값 초기화
            // (setter에서 OnPropertyChanged가 호출되므로 별도 호출 필요 없음)
            Brightness = 0;
            Contrast = 0;

            // ⭐️ 효과 갱신 (UpdateImageEffects가 DefectImage를 설정함)
            UpdateImageEffects();
        }

        public void ClearImage()
        {
            _originalDefectImage = null; // ⭐️ 원본 이미지도 클리어
            DefectImage = null;
            PixelScale = 0;
        }

        public void UpdateFromKlarf(KlarfModel klarf)
        {
            ClearImage();
        }

        // ⭐️ --- [수정] ResetView 메서드 추가 (사용자 요청 반영) ---
        /// <summary>
        /// 뷰의 상태(밝기, 대비, 줌, 패닝)를 모두 초기화합니다.
        /// </summary>
        private void ResetView()
        {
            // 1. ViewModel의 상태(밝기/대비)를 리셋합니다.
            //    프로퍼티 setter가 UpdateImageEffects()를 자동으로 호출하여
            //    이미지를 원본으로 되돌립니다.
            Brightness = 0;
            Contrast = 0;

            // 2. View에게 줌/패닝 리셋을 요청합니다.
            //    (이 이벤트는 xaml.cs 파일이 받아서 처리합니다)
            ResetViewRequested?.Invoke();
        }

        // ⭐️ --- [추가] UpdateImageEffects 메서드 ---
        /// <summary>
        /// 현재 Brightness와 Contrast 값을 기준으로
        /// 원본 이미지에 효과를 적용하여 DefectImage를 갱신합니다.
        /// </summary>
        private void UpdateImageEffects()
        {
            if (_originalDefectImage == null)
            {
                DefectImage = null;
                return;
            }

            // 밝기/대비가 모두 0(기본값)이면 원본 이미지를 그대로 표시
            if (Brightness == 0 && Contrast == 0)
            {
                DefectImage = _originalDefectImage;
            }
            else
            {
                // ⭐️ ImageService를 호출하여 효과가 적용된 이미지를 받아옴
                DefectImage = _imageService.ApplyBrightnessContrast(_originalDefectImage, Brightness, Contrast);
            }
        }
        // ⭐️ --- [추가 끝] ---
    }
}