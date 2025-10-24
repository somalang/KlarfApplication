using System;
using System.Collections.Generic; // ⭐️ [추가] Dictionary
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
            if (e.PropertyName == nameof(WaferViewModel.Dies))
            {
                DrawWaferMap();
            }
        }

        #region 줌/패닝 이벤트 및 로직
        private void OnZoomInRequested() => DoZoom(1.2);
        private void OnZoomOutRequested() => DoZoom(1 / 1.2);
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
            if (vm == null || vm.Dies == null || !vm.Dies.Any()) return;

            Point zoomCenter = center ?? new Point(WaferMapCanvas.ActualWidth / 2, WaferMapCanvas.ActualHeight / 2);
            WaferMapCanvas.RenderTransformOrigin = new Point(zoomCenter.X / WaferMapCanvas.ActualWidth, zoomCenter.Y / WaferMapCanvas.ActualHeight);

            _scaleTransform.ScaleX *= factor;
            _scaleTransform.ScaleY *= factor;
        }

        private void WaferMapCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double factor = e.Delta > 0 ? 1.2 : (1 / 1.2);
            DoZoom(factor, e.GetPosition(WaferMapCanvas));
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
            else
            {
                _panStartPoint = e.GetPosition(this);
                WaferMapCanvas.CaptureMouse();
                WaferMapCanvas.Cursor = Cursors.Hand;
            }
            e.Handled = true;
        }

        private void WaferMapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_panStartPoint.HasValue)
            {
                Point currentPoint = e.GetPosition(this);
                Vector delta = currentPoint - _panStartPoint.Value;
                _translateTransform.X += delta.X;
                _translateTransform.Y += delta.Y;
                _panStartPoint = currentPoint;
            }
        }

        private void WaferMapCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _panStartPoint = null;
            WaferMapCanvas.ReleaseMouseCapture();
            WaferMapCanvas.Cursor = Cursors.Arrow;
            e.Handled = true;
        }
        #endregion

        #region DrawWaferMap
        private void DrawWaferMap()
        {
            WaferMapCanvas.Children.Clear();
            _dieRectangles.Clear();

            if (!(DataContext is WaferViewModel viewModel) || viewModel.Dies == null || !viewModel.Dies.Any())
                return;

            // --- 좌표 계산 준비 ---
            var minRow = viewModel.Dies.Min(d => d.Row);
            var maxRow = viewModel.Dies.Max(d => d.Row);
            var minCol = viewModel.Dies.Min(d => d.Column);
            var maxCol = viewModel.Dies.Max(d => d.Column);

            int numRows = maxRow - minRow + 1;
            int numCols = maxCol - minCol + 1;

            double dieActualWidth = viewModel.Wafer?.DieWidth ?? 1.0;
            double dieActualHeight = viewModel.Wafer?.DieHeight ?? 1.0;

            double canvasWidth = WaferMapCanvas.Width;
            double canvasHeight = WaferMapCanvas.Height;
            double dieRenderWidth = canvasWidth / numRows * 0.95;
            double dieRenderHeight = canvasHeight / numCols * 0.95;

            string orientation = viewModel.Wafer?.Orientation?.ToUpper() ?? "DOWN";

            double totalMapWidth = numRows * dieRenderWidth / 0.95;
            double totalMapHeight = numCols * dieRenderHeight / 0.95;
            double offsetX = (canvasWidth - totalMapWidth) / 2;
            double offsetY = (canvasHeight - totalMapHeight) / 2;

            // --- Die 및 Defect 그리기 ---
            foreach (var die in viewModel.Dies)
            {
                // 1. Die 사각형 위치 계산
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

                // 2. Die 사각형 생성
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
                _dieRectangles[die.Key] = rect;

                // 3. 해당 Die의 Defect 그리기
                if (!die.IsGood && viewModel.AllDefects != null)
                {
                    var defectsInDie = viewModel.AllDefects.Where(def => def.Row == die.Row && def.Column == die.Column);
                    foreach (var defect in defectsInDie)
                    {
                        // --- ✨ 수정된 Defect 좌표 계산 로직 ---

                        // DieOrigin 0,0 이므로 Die의 왼쪽 하단이 기준점.
                        // XREL은 0 ~ DieActualWidth 범위, YREL은 0 ~ DieActualHeight 범위로 가정.

                        // X 좌표: Die 왼쪽부터 XREL 비율만큼 이동
                        double defectCanvasX = left + (defect.XCoord / dieActualWidth) * dieRenderWidth;

                        double defectCanvasY;
                        // Orientation과 YREL 방향에 따른 Y 좌표 계산
                        if (orientation == "UP")
                        {
                            // UP 방향: Canvas Y는 아래로 증가, YREL은 위로 증가 가정 -> Die 상단에서 YREL 비율만큼 아래로
                            defectCanvasY = top + (1.0 - (defect.YCoord / dieActualHeight)) * dieRenderHeight;
                            // 만약 YREL이 아래로 증가한다면: defectCanvasY = top + (defect.YCoord / dieActualHeight) * dieRenderHeight;
                        }
                        else // DOWN (기본값, Canvas Y는 아래로 증가, Wafer Y도 아래로 증가 (Y축 반전))
                        {
                            // DOWN 방향: Canvas Y는 아래로 증가, YREL은 위로 증가 가정 -> Die 하단에서 YREL 비율만큼 위로
                            defectCanvasY = (top + dieRenderHeight) - (defect.YCoord / dieActualHeight) * dieRenderHeight;
                            // 만약 YREL이 아래로 증가한다면: defectCanvasY = top + (defect.YCoord / dieActualHeight) * dieRenderHeight;
                        }

                        // --- ✨ 수정 끝 ---


                        // Defect 점 생성 (작은 노란색 원)
                        var defectMarker = new Ellipse
                        {
                            Width = 4,
                            Height = 4,
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