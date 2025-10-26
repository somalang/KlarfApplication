using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using KlarfApplication.ViewModel;

namespace KlarfApplication.View
{
    public partial class WaferMapViewer : UserControl
    {
        // 줌/패닝 변수
        private readonly ScaleTransform _scaleTransform;
        private readonly TranslateTransform _translateTransform;
        private Point? _panStartPoint = null;

        // DieViewModel과 Rectangle 매핑
        private readonly Dictionary<string, Rectangle> _dieRectangles = new();

        public WaferMapViewer()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;

            _scaleTransform = CanvasScaleTransform;
            _translateTransform = CanvasTranslateTransform;

            this.SizeChanged += (s, e) => DrawWaferMap();
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

        #region 줌/패닝 이벤트 및 로직

        // ⭐️ [수정됨 - Goal 1]
        // 현재 화면(Viewport)의 중심점을 캔버스(WaferMapCanvas) 내부 좌표로 변환하여 반환합니다.
        private Point GetVisibleCenterInCanvasCoordinates()
        {
            // 1. Viewport(UserControl)의 중심점
            Point viewportCenter = new Point(this.ActualWidth / 2, this.ActualHeight / 2);

            // 2. 이 중심점을 WaferMapCanvas의 좌표계로 변환
            // (현재 화면 중앙에 캔버스의 어느 지점이 와 있는지 확인)
            return this.TranslatePoint(viewportCenter, WaferMapCanvas);
        }


        // ⭐️ [수정됨 - Goal 1]
        // 버튼 줌 요청 시, 캔버스 좌표계 기준의 현재 뷰 중심을 사용합니다.
        private void OnZoomInRequested()
        {
            DoZoom(1.2, GetVisibleCenterInCanvasCoordinates());
        }

        // ⭐️ [수정됨 - Goal 1]
        private void OnZoomOutRequested()
        {
            DoZoom(1 / 1.2, GetVisibleCenterInCanvasCoordinates());
        }

        private void OnResetViewRequested()
        {
            _scaleTransform.ScaleX = 1.0;
            _scaleTransform.ScaleY = 1.0;
            _translateTransform.X = 0.0;
            _translateTransform.Y = 0.0;
            WaferMapCanvas.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        private void DoZoom(double factor, Point? center = null)
        {
            var vm = DataContext as WaferViewModel;
            if (vm == null || vm.Dies == null || !vm.Dies.Any() || WaferMapCanvas.ActualWidth == 0 || WaferMapCanvas.ActualHeight == 0) return;

            Point zoomCenter = center ?? new Point(WaferMapCanvas.ActualWidth / 2, WaferMapCanvas.ActualHeight / 2);

            WaferMapCanvas.RenderTransformOrigin = new Point(
                zoomCenter.X / WaferMapCanvas.ActualWidth,
                zoomCenter.Y / WaferMapCanvas.ActualHeight);

            _scaleTransform.ScaleX *= factor;
            _scaleTransform.ScaleY *= factor;

            // ⭐️ [추가됨 - Goal 2] 줌 직후 경계 검사
            ClampPan();
        }

        // ⭐️ [신규 추가 - Goal 2]
        /// <summary>
        /// 줌 배율이 1.0 미만이 되지 않도록 하고,
        /// 캔버스가 뷰포트 밖으로 나가지 않도록 패닝(Translate)을 제한합니다.
        /// </summary>
        private void ClampPan()
        {
            double W = WaferMapCanvas.ActualWidth;
            double H = WaferMapCanvas.ActualHeight;
            if (W == 0 || H == 0) return; // 렌더링 전이면 중단

            double sx = _scaleTransform.ScaleX;
            double sy = _scaleTransform.ScaleY;

            // 1. 줌 배율이 1.0 (100%) 미만이 되지 않도록 강제
            if (sx < 1.0) sx = 1.0;
            if (sy < 1.0) sy = 1.0;

            _scaleTransform.ScaleX = sx;
            _scaleTransform.ScaleY = sy;

            // 2. 줌 배율이 1.0이면 패닝(이동)을 0으로 리셋
            if (sx == 1.0 && sy == 1.0)
            {
                _translateTransform.X = 0.0;
                _translateTransform.Y = 0.0;
                return;
            }

            // 3. 줌 배율이 1.0보다 클 때, 캔버스 경계를 계산하여 패닝 제한
            Point origin = WaferMapCanvas.RenderTransformOrigin; // (ox, oy) 0-1 범위
            double ox = origin.X;
            double oy = origin.Y;

            // 캔버스 경계 계산 (자세한 유도 과정은 생략)
            // MaxTx: 캔버스 왼쪽 경계가 뷰포트 왼쪽 경계를 넘지 않게 (양수)
            double maxTx = W * ox * (sx - 1);
            // MinTx: 캔버스 오른쪽 경계가 뷰포트 오른쪽 경계를 넘지 않게 (음수)
            double minTx = W * (ox - 1) * (sx - 1);

            // MaxTy: 캔버스 위쪽 경계가 뷰포트 위쪽 경계를 넘지 않게 (양수)
            double maxTy = H * oy * (sy - 1);
            // MinTy: 캔버스 아래쪽 경계가 뷰포트 아래쪽 경계를 넘지 않게 (음수)
            double minTy = H * (oy - 1) * (sy - 1);

            // 현재 이동 값이 경계를 넘으면 경계 값으로 강제 설정
            if (_translateTransform.X > maxTx) _translateTransform.X = maxTx;
            if (_translateTransform.X < minTx) _translateTransform.X = minTx;

            if (_translateTransform.Y > maxTy) _translateTransform.Y = maxTy;
            if (_translateTransform.Y < minTy) _translateTransform.Y = minTy;
        }


        // 휠 줌 (수정 없음)
        private void WaferMapCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double factor = e.Delta > 0 ? 1.2 : (1 / 1.2);
            DoZoom(factor, e.GetPosition(WaferMapCanvas));
            e.Handled = true;
        }

        // LMB (클릭) (수정 없음)
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

        // LMB UP (수정 없음)
        private void WaferMapCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (WaferMapCanvas.IsMouseCaptured && _panStartPoint == null)
            {
                WaferMapCanvas.ReleaseMouseCapture();
                WaferMapCanvas.Cursor = Cursors.Arrow;
            }
            e.Handled = true;
        }

        // RMB (패닝 시작) (수정 없음)
        private void WaferMapCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _panStartPoint = e.GetPosition(this); // UserControl 기준
            WaferMapCanvas.CaptureMouse();
            WaferMapCanvas.Cursor = Cursors.Hand;
            e.Handled = true;
        }

        // ⭐️ [수정됨 - Goal 2]
        // 마우스 이동 (패닝 시 경계 검사 추가)
        private void WaferMapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_panStartPoint.HasValue && e.RightButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(this);
                Vector delta = currentPoint - _panStartPoint.Value;
                _translateTransform.X += delta.X;
                _translateTransform.Y += delta.Y;
                _panStartPoint = currentPoint;

                // ⭐️ [추가됨 - Goal 2] 패닝 직후 경계 검사
                ClampPan();
            }
        }

        // RMB UP (패닝 종료) (수정 없음)
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

        #region DrawWaferMap (수정 없음)
        // (기존 DrawWaferMap 메서드 코드는 변경할 필요가 없습니다. 그대로 두세요.)
        private void DrawWaferMap()
        {
            WaferMapCanvas.Children.Clear();
            _dieRectangles.Clear();

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
                else // DOWN
                {
                    top = (numCols - 1 - yIndex) * (dieRenderHeight / 0.95) + offsetY;
                }

                Brush fillBrush = die.IsGood ? Brushes.DarkGray : new SolidColorBrush(Color.FromRgb(0xE3, 0x04, 0x13));
                var rect = new Rectangle
                {
                    Width = dieRenderWidth,
                    Height = dieRenderHeight,
                    Fill = fillBrush,
                    Stroke = Brushes.White,
                    StrokeThickness = 0.5,
                    Tag = die
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
                        else // DOWN
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
        }
        #endregion

        #region Die 하이라이트 (수정 없음)
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