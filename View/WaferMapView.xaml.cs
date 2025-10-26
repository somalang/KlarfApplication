using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using KlarfApplication.ViewModel;

namespace KlarfApplication.View
{
    public partial class WaferMapViewer : UserControl
    {
        private readonly ScaleTransform _scaleTransform;
        private readonly TranslateTransform _translateTransform;
        private Point? _panStartPoint = null;
        private readonly Dictionary<string, Rectangle> _dieRectangles = new();
        private readonly DispatcherTimer _viewportUpdateTimer;
        private bool _isMiniMapVisible = false;

        // Die 영역 경계 저장 (줌 제한용)
        private Rect _dieMapBounds = Rect.Empty;

        public WaferMapViewer()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;

            _scaleTransform = CanvasScaleTransform;
            _translateTransform = CanvasTranslateTransform;

            this.SizeChanged += (s, e) => DrawWaferMap();

            // 뷰포트 업데이트 타이머 설정 (부드러운 업데이트를 위해)
            _viewportUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60fps
            };
            _viewportUpdateTimer.Tick += (s, e) => UpdateViewportIndicators();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is WaferViewModel oldVm)
            {
                oldVm.PropertyChanged -= OnViewModelPropertyChanged;
                oldVm.ZoomInRequested -= OnZoomInRequested;
                oldVm.ZoomOutRequested -= OnZoomOutRequested;
                oldVm.ResetViewRequested -= OnResetViewRequested;
            }
            if (e.NewValue is WaferViewModel newVm)
            {
                newVm.PropertyChanged += OnViewModelPropertyChanged;
                newVm.ZoomInRequested += OnZoomInRequested;
                newVm.ZoomOutRequested += OnZoomOutRequested;
                newVm.ResetViewRequested += OnResetViewRequested;
                DrawWaferMap();
            }
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WaferViewModel.Dies) || e.PropertyName == nameof(WaferViewModel.Wafer))
            {
                DrawWaferMap();
                OnResetViewRequested();
            }
        }

        #region 미니맵 토글
        private void ToggleMiniMap_Click(object sender, RoutedEventArgs e)
        {
            _isMiniMapVisible = !_isMiniMapVisible;
            MiniMapBorder.Visibility = _isMiniMapVisible ? Visibility.Visible : Visibility.Collapsed;

            if (_isMiniMapVisible)
            {
                DrawMiniMap();
                UpdateViewportIndicators();
                _viewportUpdateTimer.Start();
            }
            else
            {
                _viewportUpdateTimer.Stop();
            }
        }
        #endregion

        #region 줌/패닝 이벤트 및 로직
        private Point GetVisibleCenterInCanvasCoordinates()
        {
            Point viewportCenter = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
            return this.TranslatePoint(viewportCenter, WaferMapCanvas);
        }

        /// <summary>
        /// Die 맵의 중심점을 캔버스 좌표로 반환
        /// </summary>
        private Point GetDieMapCenter()
        {
            if (_dieMapBounds.IsEmpty)
                return new Point(WaferMapCanvas.ActualWidth / 2, WaferMapCanvas.ActualHeight / 2);

            return new Point(
                _dieMapBounds.Left + _dieMapBounds.Width / 2,
                _dieMapBounds.Top + _dieMapBounds.Height / 2
            );
        }

        private void OnZoomInRequested()
        {
            // Die 맵 중심을 기준으로 줌
            DoZoom(1.2, GetDieMapCenter());
        }

        private void OnZoomOutRequested()
        {
            // Die 맵 중심을 기준으로 줌
            DoZoom(1 / 1.2, GetDieMapCenter());
        }

        private void OnResetViewRequested()
        {
            _scaleTransform.ScaleX = 1.0;
            _scaleTransform.ScaleY = 1.0;
            _translateTransform.X = 0.0;
            _translateTransform.Y = 0.0;
            WaferMapCanvas.RenderTransformOrigin = new Point(0.5, 0.5);
            UpdateViewportIndicators();
        }

        private void DoZoom(double factor, Point? center = null)
        {
            var vm = DataContext as WaferViewModel;
            if (vm == null || vm.Dies == null || !vm.Dies.Any() || WaferMapCanvas.ActualWidth == 0 || WaferMapCanvas.ActualHeight == 0) return;

            // Die 맵 중심을 기본 줌 중심으로 사용
            Point zoomCenter = center ?? GetDieMapCenter();

            // Die 맵 경계 체크: Die가 있는 영역만 확대되도록 제한
            if (!_dieMapBounds.IsEmpty)
            {
                // 줌 중심이 Die 맵 경계 안에 있는지 확인
                if (!_dieMapBounds.Contains(zoomCenter))
                {
                    // Die 맵 경계 밖이면 가장 가까운 경계 지점으로 조정
                    zoomCenter.X = Math.Max(_dieMapBounds.Left, Math.Min(_dieMapBounds.Right, zoomCenter.X));
                    zoomCenter.Y = Math.Max(_dieMapBounds.Top, Math.Min(_dieMapBounds.Bottom, zoomCenter.Y));
                }
            }

            WaferMapCanvas.RenderTransformOrigin = new Point(
                zoomCenter.X / WaferMapCanvas.ActualWidth,
                zoomCenter.Y / WaferMapCanvas.ActualHeight);

            double newScaleX = _scaleTransform.ScaleX * factor;
            double newScaleY = _scaleTransform.ScaleY * factor;

            // Die 맵 기준 최대 줌 제한 (Die가 화면을 벗어나지 않도록)
            if (!_dieMapBounds.IsEmpty && factor > 1.0) // 줌인할 때만 체크
            {
                double maxScaleX = WaferMapCanvas.ActualWidth / _dieMapBounds.Width * 0.95;
                double maxScaleY = WaferMapCanvas.ActualHeight / _dieMapBounds.Height * 0.95;
                double maxScale = Math.Min(maxScaleX, maxScaleY) * 5; // 최대 5배까지 확대

                newScaleX = Math.Min(newScaleX, maxScale);
                newScaleY = Math.Min(newScaleY, maxScale);
            }

            _scaleTransform.ScaleX = newScaleX;
            _scaleTransform.ScaleY = newScaleY;

            ClampPan();
            UpdateViewportIndicators();
        }

        private void ClampPan()
        {
            double W = WaferMapCanvas.ActualWidth;
            double H = WaferMapCanvas.ActualHeight;
            if (W == 0 || H == 0) return;

            double sx = _scaleTransform.ScaleX;
            double sy = _scaleTransform.ScaleY;

            if (sx < 1.0) sx = 1.0;
            if (sy < 1.0) sy = 1.0;

            _scaleTransform.ScaleX = sx;
            _scaleTransform.ScaleY = sy;

            if (sx == 1.0 && sy == 1.0)
            {
                _translateTransform.X = 0.0;
                _translateTransform.Y = 0.0;
                return;
            }

            Point origin = WaferMapCanvas.RenderTransformOrigin;
            double ox = origin.X;
            double oy = origin.Y;

            double maxTx = W * ox * (sx - 1);
            double minTx = W * (ox - 1) * (sx - 1);
            double maxTy = H * oy * (sy - 1);
            double minTy = H * (oy - 1) * (sy - 1);

            if (_translateTransform.X > maxTx) _translateTransform.X = maxTx;
            if (_translateTransform.X < minTx) _translateTransform.X = minTx;
            if (_translateTransform.Y > maxTy) _translateTransform.Y = maxTy;
            if (_translateTransform.Y < minTy) _translateTransform.Y = minTy;
        }

        private void WaferMapCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double factor = e.Delta > 0 ? 1.2 : (1 / 1.2);

            // 마우스 위치가 Die 맵 안에 있으면 해당 위치 기준, 아니면 Die 맵 중심 기준
            Point mousePos = e.GetPosition(WaferMapCanvas);
            Point zoomCenter = _dieMapBounds.Contains(mousePos) ? mousePos : GetDieMapCenter();

            DoZoom(factor, zoomCenter);
            e.Handled = true;
        }

        private void WaferMapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as WaferViewModel;
            if (vm == null || vm.Dies == null || !vm.Dies.Any()) return;

            if (e.OriginalSource is Rectangle rect && rect.Tag is DieViewModel die)
            {
                vm.SelectedDie = die;
            }
            else if (e.OriginalSource is Canvas)
            {
                vm.SelectedDie = null;
            }
            e.Handled = true;
        }

        private void WaferMapCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (WaferMapCanvas.IsMouseCaptured && _panStartPoint == null)
            {
                WaferMapCanvas.ReleaseMouseCapture();
                WaferMapCanvas.Cursor = Cursors.Arrow;
            }
            e.Handled = true;
        }

        private void WaferMapCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _panStartPoint = e.GetPosition(this);
            WaferMapCanvas.CaptureMouse();
            WaferMapCanvas.Cursor = Cursors.Hand;
            e.Handled = true;
        }

        private void WaferMapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_panStartPoint.HasValue && e.RightButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(this);
                Vector delta = currentPoint - _panStartPoint.Value;
                _translateTransform.X += delta.X;
                _translateTransform.Y += delta.Y;
                _panStartPoint = currentPoint;
                ClampPan();
                UpdateViewportIndicators();
            }
        }

        private void WaferMapCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _panStartPoint = null;
            if (WaferMapCanvas.IsMouseCaptured && e.LeftButton != MouseButtonState.Pressed)
            {
                WaferMapCanvas.ReleaseMouseCapture();
                WaferMapCanvas.Cursor = Cursors.Arrow;
            }
            e.Handled = true;
        }
        #endregion

        #region DrawWaferMap
        private void DrawWaferMap()
        {
            WaferMapCanvas.Children.Clear();
            _dieRectangles.Clear();
            _dieMapBounds = Rect.Empty; // 초기화

            if (!(DataContext is WaferViewModel viewModel) || viewModel.Dies == null || !viewModel.Dies.Any())
                return;

            double canvasWidth = WaferMapCanvas.ActualWidth;
            double canvasHeight = WaferMapCanvas.ActualHeight;

            if (canvasWidth == 0 || canvasHeight == 0)
                return;

            var minRow = viewModel.Dies.Min(d => d.Row);
            var maxRow = viewModel.Dies.Max(d => d.Row);
            var minCol = viewModel.Dies.Min(d => d.Column);
            var maxCol = viewModel.Dies.Max(d => d.Column);

            int numRows = maxRow - minRow + 1;
            int numCols = maxCol - minCol + 1;

            double dieActualWidth = viewModel.Wafer?.DieWidth ?? 1.0;
            double dieActualHeight = viewModel.Wafer?.DieHeight ?? 1.0;

            if (dieActualWidth == 0) dieActualWidth = 1.0;
            if (dieActualHeight == 0) dieActualHeight = 1.0;
            if (numRows == 0) numRows = 1;
            if (numCols == 0) numCols = 1;

            double dieRenderWidth = canvasWidth / numRows * 0.95;
            double dieRenderHeight = canvasHeight / numCols * 0.95;

            double aspectRatioActual = dieActualWidth / dieActualHeight;
            if (dieRenderHeight == 0) dieRenderHeight = 0.001;
            double aspectRatioRender = dieRenderWidth / dieRenderHeight;

            if (aspectRatioRender > aspectRatioActual)
            {
                if (aspectRatioActual > 0)
                    dieRenderWidth = dieRenderHeight * aspectRatioActual;
            }
            else
            {
                if (aspectRatioActual > 0)
                    dieRenderHeight = dieRenderWidth / aspectRatioActual;
            }

            string orientation = viewModel.Wafer?.Orientation?.ToUpper() ?? "DOWN";

            double totalMapWidth = numRows * (dieRenderWidth / 0.95);
            double totalMapHeight = numCols * (dieRenderHeight / 0.95);
            double offsetX = (canvasWidth - totalMapWidth) / 2;
            double offsetY = (canvasHeight - totalMapHeight) / 2;

            // Die 맵 경계 계산 (첫 번째와 마지막 Die 기준)
            double minLeft = double.MaxValue, minTop = double.MaxValue;
            double maxRight = double.MinValue, maxBottom = double.MinValue;

            foreach (var die in viewModel.Dies)
            {
                double left, top;
                double xIndex = die.Row - minRow;
                double yIndex = die.Column - minCol;

                left = xIndex * (dieRenderWidth / 0.95) + offsetX;

                if (orientation == "UP")
                {
                    top = yIndex * (dieRenderHeight / 0.95) + offsetY;
                }
                else
                {
                    top = (numCols - 1 - yIndex) * (dieRenderHeight / 0.95) + offsetY;
                }

                // Die 맵 경계 업데이트
                minLeft = Math.Min(minLeft, left);
                minTop = Math.Min(minTop, top);
                maxRight = Math.Max(maxRight, left + dieRenderWidth);
                maxBottom = Math.Max(maxBottom, top + dieRenderHeight);

                Brush fillBrush = die.IsGood ? Brushes.DarkGray : new SolidColorBrush(Color.FromRgb(0xE3, 0x04, 0x13));
                var rect = new Rectangle
                {
                    Width = dieRenderWidth,
                    Height = dieRenderHeight,
                    Fill = fillBrush,
                    Stroke = Brushes.White,
                    StrokeThickness = 0.5,
                    Tag = die,
                    ToolTip = $"Die Position: [{die.Row}, {die.Column}]\nDefect Count: {die.DefectCount}\nStatus: {die.DieType}"
                };

                Canvas.SetLeft(rect, left);
                Canvas.SetTop(rect, top);

                rect.MouseEnter += Die_MouseEnter;
                rect.MouseLeave += Die_MouseLeave;

                WaferMapCanvas.Children.Add(rect);
                if (die.Key != null)
                    _dieRectangles[die.Key] = rect;

                if (!die.IsGood && viewModel.AllDefects != null)
                {
                    var defectsInDie = viewModel.AllDefects.Where(def => def.Row == die.Row && def.Column == die.Column);
                    foreach (var defect in defectsInDie)
                    {
                        if (dieActualWidth == 0 || dieActualHeight == 0) continue;

                        double defectCanvasX = left + (defect.XCoord / dieActualWidth) * dieRenderWidth;
                        double defectCanvasY;

                        if (orientation == "UP")
                        {
                            defectCanvasY = top + (1.0 - (defect.YCoord / dieActualHeight)) * dieRenderHeight;
                        }
                        else
                        {
                            defectCanvasY = (top + dieRenderHeight) - (defect.YCoord / dieActualHeight) * dieRenderHeight;
                        }

                        var defectMarker = new Ellipse
                        {
                            Width = 2,
                            Height = 2,
                            Fill = Brushes.Yellow,
                            RenderTransform = new ScaleTransform(1.0 / _scaleTransform.ScaleX, 1.0 / _scaleTransform.ScaleY),
                            RenderTransformOrigin = new Point(0.5, 0.5)
                        };

                        Canvas.SetLeft(defectMarker, defectCanvasX - defectMarker.Width / 2);
                        Canvas.SetTop(defectMarker, defectCanvasY - defectMarker.Height / 2);

                        WaferMapCanvas.Children.Add(defectMarker);
                    }
                }
            }

            // Die 맵 경계 저장
            if (minLeft != double.MaxValue && minTop != double.MaxValue)
            {
                _dieMapBounds = new Rect(minLeft, minTop, maxRight - minLeft, maxBottom - minTop);
            }

            if (_isMiniMapVisible)
            {
                DrawMiniMap();
                UpdateViewportIndicators();
            }
        }
        #endregion

        #region 미니맵 그리기
        private void DrawMiniMap()
        {
            MiniMapCanvas.Children.Clear();

            if (!(DataContext is WaferViewModel viewModel) || viewModel.Dies == null || !viewModel.Dies.Any())
                return;

            double miniMapWidth = MiniMapCanvas.ActualWidth > 0 ? MiniMapCanvas.ActualWidth : 116;
            double miniMapHeight = MiniMapCanvas.ActualHeight > 0 ? MiniMapCanvas.ActualHeight : 116;

            var minRow = viewModel.Dies.Min(d => d.Row);
            var maxRow = viewModel.Dies.Max(d => d.Row);
            var minCol = viewModel.Dies.Min(d => d.Column);
            var maxCol = viewModel.Dies.Max(d => d.Column);

            int numRows = maxRow - minRow + 1;
            int numCols = maxCol - minCol + 1;

            double dieActualWidth = viewModel.Wafer?.DieWidth ?? 1.0;
            double dieActualHeight = viewModel.Wafer?.DieHeight ?? 1.0;

            if (dieActualWidth == 0) dieActualWidth = 1.0;
            if (dieActualHeight == 0) dieActualHeight = 1.0;

            double dieWidth = miniMapWidth / numRows * 0.9;
            double dieHeight = miniMapHeight / numCols * 0.9;

            // 종횡비 유지
            double aspectRatioActual = dieActualWidth / dieActualHeight;
            double aspectRatioRender = dieWidth / dieHeight;

            if (aspectRatioRender > aspectRatioActual)
            {
                if (aspectRatioActual > 0)
                    dieWidth = dieHeight * aspectRatioActual;
            }
            else
            {
                if (aspectRatioActual > 0)
                    dieHeight = dieWidth / aspectRatioActual;
            }

            string orientation = viewModel.Wafer?.Orientation?.ToUpper() ?? "DOWN";

            double totalMapWidth = numRows * (dieWidth / 0.9);
            double totalMapHeight = numCols * (dieHeight / 0.9);
            double offsetX = (miniMapWidth - totalMapWidth) / 2;
            double offsetY = (miniMapHeight - totalMapHeight) / 2;

            foreach (var die in viewModel.Dies)
            {
                double xIndex = die.Row - minRow;
                double yIndex = die.Column - minCol;

                double left = xIndex * (dieWidth / 0.9) + offsetX;
                double top;

                if (orientation == "UP")
                {
                    top = yIndex * (dieHeight / 0.9) + offsetY;
                }
                else
                {
                    top = (numCols - 1 - yIndex) * (dieHeight / 0.9) + offsetY;
                }

                Brush fillBrush = die.IsGood ? Brushes.DarkGray : new SolidColorBrush(Color.FromRgb(0xE3, 0x04, 0x13));
                var rect = new Rectangle
                {
                    Width = dieWidth,
                    Height = dieHeight,
                    Fill = fillBrush,
                    Stroke = Brushes.White,
                    StrokeThickness = 0.3
                };

                Canvas.SetLeft(rect, left);
                Canvas.SetTop(rect, top);
                MiniMapCanvas.Children.Add(rect);
            }
        }
        #endregion

        #region 뷰포트 인디케이터 업데이트
        private void UpdateViewportIndicators()
        {
            if (!_isMiniMapVisible) return;

            double canvasWidth = WaferMapCanvas.ActualWidth;
            double canvasHeight = WaferMapCanvas.ActualHeight;

            if (canvasWidth == 0 || canvasHeight == 0) return;

            // 현재 보이는 영역 계산 (UserControl 좌표계)
            Rect visibleRect = new Rect(0, 0, this.ActualWidth, this.ActualHeight);

            // 캔버스 좌표계로 변환
            Point topLeft = this.TranslatePoint(visibleRect.TopLeft, WaferMapCanvas);
            Point bottomRight = this.TranslatePoint(visibleRect.BottomRight, WaferMapCanvas);

            // 미니맵의 뷰포트 사각형 업데이트
            double miniMapWidth = MiniMapCanvas.ActualWidth > 0 ? MiniMapCanvas.ActualWidth : 116;
            double miniMapHeight = MiniMapCanvas.ActualHeight > 0 ? MiniMapCanvas.ActualHeight : 116;

            // 정규화된 좌표 계산 (0~1 범위)
            double normalizedLeft = Math.Max(0, Math.Min(1, topLeft.X / canvasWidth));
            double normalizedTop = Math.Max(0, Math.Min(1, topLeft.Y / canvasHeight));
            double normalizedRight = Math.Max(0, Math.Min(1, bottomRight.X / canvasWidth));
            double normalizedBottom = Math.Max(0, Math.Min(1, bottomRight.Y / canvasHeight));

            // 미니맵 좌표로 변환
            double miniLeft = normalizedLeft * miniMapWidth;
            double miniTop = normalizedTop * miniMapHeight;
            double miniWidth = (normalizedRight - normalizedLeft) * miniMapWidth;
            double miniHeight = (normalizedBottom - normalizedTop) * miniMapHeight;

            Canvas.SetLeft(MiniMapViewport, miniLeft);
            Canvas.SetTop(MiniMapViewport, miniTop);
            MiniMapViewport.Width = miniWidth;
            MiniMapViewport.Height = miniHeight;
        }
        #endregion

        #region Die 하이라이트
        private void Die_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                rect.StrokeThickness = 2;
                rect.Stroke = Brushes.Yellow;
            }
        }
        private void Die_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                rect.StrokeThickness = 0.5;
                rect.Stroke = Brushes.White;
            }
        }
        #endregion
    }
}