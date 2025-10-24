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

        // 밝기/대비를 위한 변수
        private BitmapSource _originalBitmap;
        private bool _isProcessing = false;

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

        private void LoadOriginalImage(BitmapSource image)
        {
            _originalBitmap = image;
            OnFitToWindowRequested();
            BrightnessSlider.ValueChanged -= BrightnessSlider_ValueChanged;
            ContrastSlider.ValueChanged -= ContrastSlider_ValueChanged;
            BrightnessSlider.Value = 0;
            ContrastSlider.Value = 0;
            BrightnessSlider.ValueChanged += BrightnessSlider_ValueChanged;
            ContrastSlider.ValueChanged += ContrastSlider_ValueChanged;
            DefectImage.Source = _originalBitmap;
            DrawingCanvas.Children.Clear();
            _ruler = null;
            _rulerText = null;
        }
        #endregion

        #region 버튼 Command / 줌 로직

        private void OnResetViewRequested() { LoadOriginalImage(_originalBitmap); }

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
            if (_originalBitmap == null) return;
            ImageGrid.RenderTransformOrigin = new Point(0.5, 0.5);
            _scaleTransform.ScaleX *= zoomFactor;
            _scaleTransform.ScaleY *= zoomFactor;
        }
        #endregion

        #region 마우스 이벤트 (LMB=Pan, RMB=Measure)

        // (공통) 휠 줌
        private void ImageGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_originalBitmap == null) return;
            double zoomFactor = e.Delta > 0 ? 1.2 : (1 / 1.2);
            Point mousePos = e.GetPosition(ImageGrid);
            ImageGrid.RenderTransformOrigin = new Point(
                mousePos.X / ImageGrid.ActualWidth,
                mousePos.Y / ImageGrid.ActualHeight);
            _scaleTransform.ScaleX *= zoomFactor;
            _scaleTransform.ScaleY *= zoomFactor;
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

            // 기존 패닝 시작 로직
            if (_originalBitmap == null || _measurementStartPoint.HasValue) return; // 측정 중일 땐 패닝 안 함
            _panStartPoint = e.GetPosition(ImageScrollViewer);
            ImageGrid.CaptureMouse();
            ImageGrid.Cursor = Cursors.Hand;
            e.Handled = true;
        }

        // RMB (측정 시작)
        private void ImageGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_originalBitmap == null || _panStartPoint.HasValue) return;
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

        #region 밝기 / 대비
        // (밝기/대비 로직은 변경 없음)
        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { ApplyEffects(); }
        private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { ApplyEffects(); }
        private void ApplyEffects()
        {
            if (_originalBitmap == null || _isProcessing) return;
            _isProcessing = true;
            try
            {
                double brightness = BrightnessSlider.Value;
                double contrast = ContrastSlider.Value;
                FormatConvertedBitmap formattedBitmap = new FormatConvertedBitmap(_originalBitmap, PixelFormats.Bgra32, null, 0);
                WriteableBitmap wBitmap = new WriteableBitmap(formattedBitmap);
                int width = wBitmap.PixelWidth; int height = wBitmap.PixelHeight; int stride = wBitmap.BackBufferStride;
                byte[] pixels = new byte[height * stride];
                wBitmap.CopyPixels(pixels, stride, 0);
                double contrastFactor = (100.0 + contrast) / 100.0;
                contrastFactor *= contrastFactor;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * stride + x * 4;
                        byte b = pixels[index]; byte g = pixels[index + 1]; byte r = pixels[index + 2];
                        double newB = ((b / 255.0 - 0.5) * contrastFactor + 0.5) * 255.0;
                        double newG = ((g / 255.0 - 0.5) * contrastFactor + 0.5) * 255.0;
                        double newR = ((r / 255.0 - 0.5) * contrastFactor + 0.5) * 255.0;
                        newB += brightness; newG += brightness; newR += brightness;
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
            finally { _isProcessing = false; }
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