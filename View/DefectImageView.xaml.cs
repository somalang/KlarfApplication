using KlarfApplication.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes; // ⭐️ [추가] Line 사용

namespace KlarfApplication.View
{
    public partial class DefectImageViewer : UserControl
    {
        // 줌/패닝(이동)을 위한 변수
        private readonly ScaleTransform _scaleTransform = new ScaleTransform();
        private readonly TranslateTransform _translateTransform = new TranslateTransform();
        private Point? _panStartPoint = null; // ⭐️ 패닝(LMB) 시작점

        // 밝기/대비를 위한 변수
        private BitmapSource _originalBitmap;
        private bool _isProcessing = false;

        // ⭐️ --- [측정 기능 추가 시작] ---
        private Point? _measurementStartPoint = null; // ⭐️ 측정(RMB) 시작점
        private Line _ruler;
        private TextBlock _rulerText;
        // ⭐️ --- [측정 기능 추가 끝] ---


        public DefectImageViewer()
        {
            InitializeComponent();

            // 1. [수정] 줌/패닝(이동)을 ImageGrid에 적용
            var group = new TransformGroup();
            group.Children.Add(_scaleTransform);
            group.Children.Add(_translateTransform);
            ImageGrid.RenderTransform = group; // ⭐️ Image -> ImageGrid로 변경

            // 2. ViewModel이 바뀔 때(새 이미지가 로드될 때)를 감지
            this.DataContextChanged += OnDataContextChanged;
        }

        #region ViewModel 연동 (이벤트 구독)

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is DefectImageViewModel oldVm)
            {
                oldVm.PropertyChanged -= OnViewModelPropertyChanged;
                oldVm.ResetViewRequested -= OnResetViewRequested;
                oldVm.FitToWindowRequested -= OnFitToWindowRequested;
                oldVm.ZoomInRequested -= OnZoomInRequested;
                oldVm.ZoomOutRequested -= OnZoomOutRequested;
            }
            if (e.NewValue is DefectImageViewModel newVm)
            {
                newVm.PropertyChanged += OnViewModelPropertyChanged;
                newVm.ResetViewRequested += OnResetViewRequested;
                newVm.FitToWindowRequested += OnFitToWindowRequested;
                newVm.ZoomInRequested += OnZoomInRequested;
                newVm.ZoomOutRequested += OnZoomOutRequested;

                LoadOriginalImage(newVm.DefectImage);
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DefectImageViewModel.DefectImage))
            {
                var viewModel = DataContext as DefectImageViewModel;
                LoadOriginalImage(viewModel?.DefectImage);
            }
        }

        // 원본 이미지를 저장하고 슬라이더와 줌/패닝/측정선을 리셋
        private void LoadOriginalImage(BitmapSource image)
        {
            _originalBitmap = image;

            // 1. 줌/패닝 리셋
            OnFitToWindowRequested(); // 화면 맞춤으로 리셋

            // 2. 슬라이더 리셋
            BrightnessSlider.ValueChanged -= BrightnessSlider_ValueChanged;
            ContrastSlider.ValueChanged -= ContrastSlider_ValueChanged;
            BrightnessSlider.Value = 0;
            ContrastSlider.Value = 0;
            BrightnessSlider.ValueChanged += BrightnessSlider_ValueChanged;
            ContrastSlider.ValueChanged += ContrastSlider_ValueChanged;

            // 3. XAML의 Image 컨트롤에 원본(또는 null)을 설정
            DefectImage.Source = _originalBitmap;

            // 4. [추가] 측정선(Ruler) 지우기
            DrawingCanvas.Children.Clear();
            _ruler = null;
            _rulerText = null;
        }

        #endregion

        #region 버튼 Command / 줌 로직

        // (버튼) 뷰 초기화
        private void OnResetViewRequested()
        {
            LoadOriginalImage(_originalBitmap);
        }

        // (버튼) 화면 맞춤
        private void OnFitToWindowRequested()
        {
            _scaleTransform.ScaleX = 1.0;
            _scaleTransform.ScaleY = 1.0;
            _translateTransform.X = 0.0;
            _translateTransform.Y = 0.0;
            // [수정] ImageGrid의 중앙을 기준으로 줌
            ImageGrid.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        // (버튼) 줌 인
        private void OnZoomInRequested()
        {
            DoZoom(1.2); // 1.2배 확대
        }

        // (버튼) 줌 아웃
        private void OnZoomOutRequested()
        {
            DoZoom(1 / 1.2); // 1.2배 축소
        }

        // (공통) 줌 로직
        private void DoZoom(double zoomFactor)
        {
            if (_originalBitmap == null) return;
            // [수정] ImageGrid의 중앙을 기준으로 줌
            ImageGrid.RenderTransformOrigin = new Point(0.5, 0.5);
            _scaleTransform.ScaleX *= zoomFactor;
            _scaleTransform.ScaleY *= zoomFactor;
        }

        #endregion

        #region 마우스 이벤트 (줌 / 패닝 / 측정)

        // [수정] 이벤트 주체: Image -> ImageGrid
        private void ImageGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_originalBitmap == null) return;

            double zoomFactor = e.Delta > 0 ? 1.2 : (1 / 1.2);
            Point mousePos = e.GetPosition(ImageGrid);

            // [수정] ImageGrid의 마우스 포인터를 기준으로 줌
            ImageGrid.RenderTransformOrigin = new Point(
                mousePos.X / ImageGrid.ActualWidth,
                mousePos.Y / ImageGrid.ActualHeight);

            _scaleTransform.ScaleX *= zoomFactor;
            _scaleTransform.ScaleY *= zoomFactor;

            e.Handled = true;
        }

        // [수정] 이벤트 주체: Image -> ImageGrid (패닝 시작)
        private void ImageGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_originalBitmap == null || _measurementStartPoint.HasValue) return; // 측정 중일 땐 패닝 안 함

            // 패닝(이동) 시작점 저장
            _panStartPoint = e.GetPosition(ImageScrollViewer); // 스크롤뷰어 기준
            ImageGrid.CaptureMouse();
            ImageGrid.Cursor = Cursors.Hand;
            e.Handled = true;
        }

        // [수정] 이벤트 주체: Image -> ImageGrid (측정 시작)
        private void ImageGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_originalBitmap == null || _panStartPoint.HasValue) return; // 패닝 중일 땐 측정 안 함

            e.Handled = true; // ContextMenu 방지

            // 1. 기존 측정선 제거
            DrawingCanvas.Children.Clear();

            // 2. 측정 시작점 (Canvas 기준 좌표)
            _measurementStartPoint = e.GetPosition(DrawingCanvas);

            // 3. 선(Ruler) 생성 및 설정
            _ruler = new Line
            {
                X1 = _measurementStartPoint.Value.X,
                Y1 = _measurementStartPoint.Value.Y,
                X2 = _measurementStartPoint.Value.X,
                Y2 = _measurementStartPoint.Value.Y,
                Stroke = Brushes.Cyan,
                StrokeThickness = 1.0 / _scaleTransform.ScaleX // ⭐️ 줌 배율 보정
            };

            // 4. 텍스트(Length) 생성 및 설정
            _rulerText = new TextBlock
            {
                Text = "0 px",
                Foreground = Brushes.Cyan,
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                // ⭐️ 줌 배율 보정 (글자가 커지지 않게)
                RenderTransform = new ScaleTransform(1.0 / _scaleTransform.ScaleX, 1.0 / _scaleTransform.ScaleY)
            };

            // 5. Canvas에 추가
            DrawingCanvas.Children.Add(_ruler);
            DrawingCanvas.Children.Add(_rulerText);
            Canvas.SetLeft(_rulerText, _measurementStartPoint.Value.X + 5);
            Canvas.SetTop(_rulerText, _measurementStartPoint.Value.Y + 5);

            ImageGrid.CaptureMouse();
        }


        // [수정] 이벤트 주체: Image -> ImageGrid (패닝/측정 중)
        private void ImageGrid_MouseMove(object sender, MouseEventArgs e)
        {
            // 1. 패닝(LMB) 로직
            if (_panStartPoint.HasValue)
            {
                Point currentPoint = e.GetPosition(ImageScrollViewer);
                Vector delta = currentPoint - _panStartPoint.Value;
                _translateTransform.X += delta.X;
                _translateTransform.Y += delta.Y;
                _panStartPoint = currentPoint;
            }
            // 2. 측정(RMB) 로직
            else if (_measurementStartPoint.HasValue && _ruler != null)
            {
                Point currentPoint = e.GetPosition(DrawingCanvas);

                // 선 끝점 업데이트
                _ruler.X2 = currentPoint.X;
                _ruler.Y2 = currentPoint.Y;

                // ⭐️ 줌 배율 보정 (선 두께)
                double currentStrokeThickness = 1.0 / _scaleTransform.ScaleX;
                _ruler.StrokeThickness = currentStrokeThickness;

                // 거리 계산 (픽셀 단위)
                double length = (_measurementStartPoint.Value - currentPoint).Length;

                // 텍스트 업데이트
                _rulerText.Text = $"{length:F1} px";
                // ⭐️ 줌 배율 보정 (글자 크기)
                _rulerText.RenderTransform = new ScaleTransform(currentStrokeThickness, currentStrokeThickness);

                // 텍스트 위치 (선의 중간점 근처)
                Canvas.SetLeft(_rulerText, (currentPoint.X + _measurementStartPoint.Value.X) / 2);
                Canvas.SetTop(_rulerText, (currentPoint.Y + _measurementStartPoint.Value.Y) / 2);
            }
        }

        // [수정] 이벤트 주체: Image -> ImageGrid (패닝 종료)
        private void ImageGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _panStartPoint = null; // 패닝 종료
            ImageGrid.ReleaseMouseCapture();
            ImageGrid.Cursor = Cursors.Arrow;
            e.Handled = true;
        }

        // [수정] 이벤트 주체: Image -> ImageGrid (측정 종료)
        private void ImageGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _measurementStartPoint = null; // 측정 종료
            ImageGrid.ReleaseMouseCapture();
            e.Handled = true;
        }


        #endregion

        #region 밝기 / 대비 (변경 없음)

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ApplyEffects();
        }

        private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ApplyEffects();
        }

        private void ApplyEffects()
        {
            if (_originalBitmap == null || _isProcessing)
                return;

            _isProcessing = true;
            try
            {
                double brightness = BrightnessSlider.Value;
                double contrast = ContrastSlider.Value;

                FormatConvertedBitmap formattedBitmap = new FormatConvertedBitmap(
                    _originalBitmap, PixelFormats.Bgra32, null, 0);

                WriteableBitmap wBitmap = new WriteableBitmap(formattedBitmap);

                int width = wBitmap.PixelWidth;
                int height = wBitmap.PixelHeight;
                int stride = wBitmap.BackBufferStride;

                byte[] pixels = new byte[height * stride];
                wBitmap.CopyPixels(pixels, stride, 0);

                double contrastFactor = (100.0 + contrast) / 100.0;
                contrastFactor *= contrastFactor;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * stride + x * 4;
                        byte b = pixels[index];
                        byte g = pixels[index + 1];
                        byte r = pixels[index + 2];

                        double newB = ((b / 255.0 - 0.5) * contrastFactor + 0.5) * 255.0;
                        double newG = ((g / 255.0 - 0.5) * contrastFactor + 0.5) * 255.0;
                        double newR = ((r / 255.0 - 0.5) * contrastFactor + 0.5) * 255.0;

                        newB += brightness;
                        newG += brightness;
                        newR += brightness;

                        pixels[index] = Clamp(newB);
                        pixels[index + 1] = Clamp(newG);
                        pixels[index + 2] = Clamp(newR);
                    }
                }
                wBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
                DefectImage.Source = wBitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplyEffects Error: {ex.Message}");
                DefectImage.Source = _originalBitmap;
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private byte Clamp(double value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)value;
        }

        #endregion
    }
}