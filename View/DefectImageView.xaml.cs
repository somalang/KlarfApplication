using KlarfApplication.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KlarfApplication.View
{
    public partial class DefectImageViewer : UserControl
    {
        // 줌/패닝(이동)을 위한 변수
        private readonly ScaleTransform _scaleTransform = new ScaleTransform();
        private readonly TranslateTransform _translateTransform = new TranslateTransform();
        private Point? _panStartPoint = null; // ⭐️ LMB (패닝)

        // ⭐️ [삭제] 밝기/대비 관련 필드 제거
        // private BitmapSource _originalBitmap;
        // private bool _isProcessing = false;

        // ⭐️ RMB (측정)
        private Point? _measurementStartPoint = null;
        private Line _ruler;
        private TextBlock _rulerText;

        public DefectImageViewer()
        {
            InitializeComponent();

            var group = new TransformGroup();
            group.Children.Add(_scaleTransform);
            group.Children.Add(_translateTransform);
            ImageGrid.RenderTransform = group;

            this.DataContextChanged += OnDataContextChanged;
        }

        #region ViewModel 연동

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
                LoadOriginalImage(newVm.DefectImage); // ⭐️ VM의 DefectImage가 변경되면 LoadOriginalImage 호출
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DefectImageViewModel.DefectImage))
            {
                var viewModel = DataContext as DefectImageViewModel;

                // ⭐️ VM에서 이미 효과가 적용된 이미지를 전달하므로,
                //    DefectImage.Source에 바로 할당합니다.
                DefectImage.Source = viewModel?.DefectImage;

                // ⭐️ VM의 DefectImage가 null일 때(이미지 클리어 시) 측정 도구를 정리합니다.
                if (viewModel?.DefectImage == null)
                {
                    DrawingCanvas.Children.Clear();
                    _ruler = null;
                    _rulerText = null;
                }
            }
        }

        private void LoadOriginalImage(BitmapSource image)
        {
            // ⭐️ [삭제] _originalBitmap = image;

            // ⭐️ [수정] 뷰 리셋 및 슬라이더 초기화 관련 코드 모두 삭제
            //    (VM이 Brightness/Contrast를 0으로 설정 -> XAML이 바인딩)
            OnFitToWindowRequested();
            // ⭐️ [삭제] BrightnessSlider.ValueChanged -= ...
            // ⭐️ [삭제] ContrastSlider.ValueChanged -= ...
            // ⭐️ [삭제] BrightnessSlider.Value = 0;
            // ⭐️ [삭제] ContrastSlider.Value = 0;
            // ⭐️ [삭제] BrightnessSlider.ValueChanged += ...
            // ⭐️ [삭제] ContrastSlider.ValueChanged += ...

            DefectImage.Source = image; // ⭐️ VM에서 받은 이미지로 설정

            DrawingCanvas.Children.Clear();
            _ruler = null;
            _rulerText = null;
        }
        #endregion

        #region 버튼 Command / 줌 로직

        // ⭐️ [수정] 
        // 이 핸들러는 VM의 ResetViewCommand -> ResetViewRequested 이벤트에 의해 호출됩니다.
        // VM은 밝기/대비를 리셋하고, 이 핸들러는 줌/패닝(View)을 리셋합니다.
        private void OnResetViewRequested()
        {
            // ⭐️ [삭제] LoadOriginalImage(_originalBitmap);
            OnFitToWindowRequested(); // ⭐️ 줌/패닝만 리셋
        }

        private void OnFitToWindowRequested()
        {
            _scaleTransform.ScaleX = 1.0; _scaleTransform.ScaleY = 1.0;
            _translateTransform.X = 0.0; _translateTransform.Y = 0.0;
            ImageGrid.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        private void OnZoomInRequested() { DoZoom(1.2); }

        private void OnZoomOutRequested() { DoZoom(1 / 1.2); }

        private void DoZoom(double zoomFactor)
        {
            // ⭐️ [수정] _originalBitmap 대신 DefectImage.Source로 확인
            if (DefectImage.Source == null) return;
            ImageGrid.RenderTransformOrigin = new Point(0.5, 0.5);

            double newScaleX = _scaleTransform.ScaleX * zoomFactor;
            double newScaleY = _scaleTransform.ScaleY * zoomFactor;

            // 최대 줌: 300% (3배), 최소 줌: 100% (1배)
            const double MAX_ZOOM = 3.0;
            const double MIN_ZOOM = 1.0;

            if (newScaleX > MAX_ZOOM) newScaleX = MAX_ZOOM;
            if (newScaleX < MIN_ZOOM) newScaleX = MIN_ZOOM;
            if (newScaleY > MAX_ZOOM) newScaleY = MAX_ZOOM;
            if (newScaleY < MIN_ZOOM) newScaleY = MIN_ZOOM;

            _scaleTransform.ScaleX = newScaleX;
            _scaleTransform.ScaleY = newScaleY;
        }
        #endregion

        #region 마우스 이벤트 (LMB=Pan, RMB=Measure)

        // (공통) 휠 줌
        private void ImageGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // ⭐️ [수정] _originalBitmap 대신 DefectImage.Source로 확인
            if (DefectImage.Source == null) return;
            double zoomFactor = e.Delta > 0 ? 1.2 : (1 / 1.2);
            Point mousePos = e.GetPosition(ImageGrid);
            ImageGrid.RenderTransformOrigin = new Point(
                mousePos.X / ImageGrid.ActualWidth,
                mousePos.Y / ImageGrid.ActualHeight);

            double newScaleX = _scaleTransform.ScaleX * zoomFactor;
            double newScaleY = _scaleTransform.ScaleY * zoomFactor;

            // (줌 제한 로직은 동일)
            const double MAX_ZOOM = 3.0;
            const double MIN_ZOOM = 1.0;
            if (newScaleX > MAX_ZOOM) newScaleX = MAX_ZOOM;
            if (newScaleX < MIN_ZOOM) newScaleX = MIN_ZOOM;
            if (newScaleY > MAX_ZOOM) newScaleY = MAX_ZOOM;
            if (newScaleY < MIN_ZOOM) newScaleY = MIN_ZOOM;

            _scaleTransform.ScaleX = newScaleX;
            _scaleTransform.ScaleY = newScaleY;
            e.Handled = true;
        }

        // LMB (패닝 시작)
        private void ImageGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // --- 측정선 지우기 ---
            if (_ruler != null)
            {
                DrawingCanvas.Children.Clear();
                _ruler = null;
                _rulerText = null;
            }
            // --- 측정선 지우기 끝 ---

            // ⭐️ [수정] _originalBitmap 대신 DefectImage.Source로 확인
            if (DefectImage.Source == null || _measurementStartPoint.HasValue) return; // 측정 중일 땐 패닝 안 함
            _panStartPoint = e.GetPosition(ImageScrollViewer);
            ImageGrid.CaptureMouse();
            ImageGrid.Cursor = Cursors.Hand;
            e.Handled = true;
        }

        // RMB (측정 시작)
        private void ImageGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // ⭐️ [수정] _originalBitmap 대신 DefectImage.Source로 확인
            if (DefectImage.Source == null || _panStartPoint.HasValue) return;
            e.Handled = true;

            DrawingCanvas.Children.Clear();
            _measurementStartPoint = e.GetPosition(DrawingCanvas);

            _ruler = new Line
            {
                X1 = _measurementStartPoint.Value.X,
                Y1 = _measurementStartPoint.Value.Y,
                X2 = _measurementStartPoint.Value.X,
                Y2 = _measurementStartPoint.Value.Y,
                Stroke = Brushes.Cyan,
                StrokeThickness = 1.0 / _scaleTransform.ScaleX
            };

            _rulerText = new TextBlock
            {
                Text = "0 px",
                Foreground = Brushes.Cyan,
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                RenderTransform = new ScaleTransform(1.0 / _scaleTransform.ScaleX, 1.0 / _scaleTransform.ScaleY)
            };

            DrawingCanvas.Children.Add(_ruler);
            DrawingCanvas.Children.Add(_rulerText);
            Canvas.SetLeft(_rulerText, _measurementStartPoint.Value.X + 5);
            Canvas.SetTop(_rulerText, _measurementStartPoint.Value.Y + 5);

            ImageGrid.CaptureMouse();
        }

        // Mouse Move (패닝/측정)
        private void ImageGrid_MouseMove(object sender, MouseEventArgs e)
        {
            // 1. 패닝(LMB)
            if (_panStartPoint.HasValue)
            {
                Point currentPoint = e.GetPosition(ImageScrollViewer);
                Vector delta = currentPoint - _panStartPoint.Value;
                _translateTransform.X += delta.X;
                _translateTransform.Y += delta.Y;
                _panStartPoint = currentPoint;
            }
            // 2. 측정(RMB)
            else if (_measurementStartPoint.HasValue && _ruler != null)
            {
                Point currentPoint = e.GetPosition(DrawingCanvas);

                _ruler.X2 = currentPoint.X;
                _ruler.Y2 = currentPoint.Y;

                double currentStrokeThickness = 1.0 / _scaleTransform.ScaleX;
                _ruler.StrokeThickness = currentStrokeThickness;

                double lengthInPixels = (_measurementStartPoint.Value - currentPoint).Length;
                string lengthText;

                var viewModel = DataContext as DefectImageViewModel;
                double pixelScale = (viewModel != null) ? viewModel.PixelScale : 0;

                if (pixelScale > 0)
                {
                    double realLength = lengthInPixels * pixelScale;
                    lengthText = $"{realLength:F2} µm ({lengthInPixels:F1} px)";
                }
                else
                {
                    lengthText = $"{lengthInPixels:F1} px";
                }
                _rulerText.Text = lengthText;

                _rulerText.RenderTransform = new ScaleTransform(currentStrokeThickness, currentStrokeThickness);
                Canvas.SetLeft(_rulerText, (currentPoint.X + _measurementStartPoint.Value.X) / 2);
                Canvas.SetTop(_rulerText, (currentPoint.Y + _measurementStartPoint.Value.Y) / 2);
            }
        }

        // LButton Up (패닝 종료)
        private void ImageGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _panStartPoint = null;
            ImageGrid.ReleaseMouseCapture();
            ImageGrid.Cursor = Cursors.Arrow;
            e.Handled = true;
        }

        // RButton Up (측정 종료)
        private void ImageGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _measurementStartPoint = null;
            ImageGrid.ReleaseMouseCapture();
            e.Handled = true;
        }
        #endregion

        // ⭐️ [삭제] 밝기 / 대비 영역 전체 삭제
        #region 밝기 / 대비 
        // (이하 모든 메서드 삭제)
        // private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { ApplyEffects(); }
        // private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { ApplyEffects(); }
        // private void ApplyEffects() { ... }
        // private byte Clamp(double value) { ... }
        #endregion
    }
}