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

            // ⭐️ 캔버스 크기가 변경될 때(예: 창 크기 조절) 맵을 다시 그리도록 이벤트 연결
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
            // ⭐️ Dies 또는 Wafer(Die 크기)가 변경되면 다시 그림
            if (e.PropertyName == nameof(WaferViewModel.Dies) || e.PropertyName == nameof(WaferViewModel.Wafer))
            {
                DrawWaferMap();
                OnResetViewRequested(); // ⭐️ 맵 다시 그릴 때 줌/패닝 리셋 추가
            }
        }

        #region 줌/패닝 이벤트 및 로직
        // ⭐️ [수정] 줌 버튼 클릭 시 현재 보이는 중심점을 기준으로 줌
        private void OnZoomInRequested()
        {
            Point center = GetCanvasCenterInControlCoordinates();
            DoZoom(1.2, center);
        }
        // ⭐️ [수정] 줌 버튼 클릭 시 현재 보이는 중심점을 기준으로 줌
        private void OnZoomOutRequested()
        {
            Point center = GetCanvasCenterInControlCoordinates();
            DoZoom(1 / 1.2, center);
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
            // ⭐️ 캔버스 크기가 0일 때 줌 방지 추가
            if (vm == null || vm.Dies == null || !vm.Dies.Any() || WaferMapCanvas.ActualWidth == 0 || WaferMapCanvas.ActualHeight == 0) return;

            // ⭐️ 마우스 위치 또는 캔버스 중심을 줌 중심으로 사용
            Point zoomCenter = center ?? new Point(WaferMapCanvas.ActualWidth / 2, WaferMapCanvas.ActualHeight / 2);

            // ⭐️ RenderTransformOrigin을 0~1 사이 값으로 설정
            WaferMapCanvas.RenderTransformOrigin = new Point(
                zoomCenter.X / WaferMapCanvas.ActualWidth,
                zoomCenter.Y / WaferMapCanvas.ActualHeight);

            _scaleTransform.ScaleX *= factor;
            _scaleTransform.ScaleY *= factor;
        }

        // ⭐️ 캔버스의 현재 보이는 중심 좌표를 UserControl 기준으로 반환하는 도우미 메서드
        private Point GetCanvasCenterInControlCoordinates()
        {
            // 캔버스 자체의 중심 (0,0 에서 Width/2, Height/2 만큼 떨어진 곳)
            Point canvasCenter = new Point(WaferMapCanvas.ActualWidth / 2, WaferMapCanvas.ActualHeight / 2);
            // 이 중심점을 현재 Transform을 적용하여 부모(UserControl) 좌표계로 변환
            return WaferMapCanvas.TranslatePoint(canvasCenter, this);
        }


        // ⭐️ 휠 줌
        private void WaferMapCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double factor = e.Delta > 0 ? 1.2 : (1 / 1.2);
            // ⭐️ DoZoom에 전달하는 중심점은 캔버스 내부 좌표여야 함
            DoZoom(factor, e.GetPosition(WaferMapCanvas));
            e.Handled = true;
        }

        // ⭐️ LMB (왼쪽 버튼) = 클릭 전용
        private void WaferMapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as WaferViewModel;
            if (vm == null || vm.Dies == null || !vm.Dies.Any()) return;

            // 1. Die (Rectangle)을 클릭한 경우
            if (e.OriginalSource is Rectangle rect && rect.Tag is DieViewModel die)
            {
                vm.SelectedDie = die;
            }
            // 2. 빈 캔버스(Canvas)를 클릭한 경우
            else if (e.OriginalSource is Canvas)
            {
                vm.SelectedDie = null; // 선택 해제
            }
            e.Handled = true;
        }

        // ⭐️ LMB UP
        private void WaferMapCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // LMB는 클릭 전용이므로 Up 이벤트에서 특별히 할 일 없음
            // (혹시 모를 마우스 캡쳐 해제)
            if (WaferMapCanvas.IsMouseCaptured && _panStartPoint == null) // 패닝 중이 아닐 때만
            {
                WaferMapCanvas.ReleaseMouseCapture();
                WaferMapCanvas.Cursor = Cursors.Arrow;
            }
            e.Handled = true;
        }

        // ⭐️ RMB (오른쪽 버튼) = 패닝 시작
        private void WaferMapCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Die든 캔버스든 상관없이 패닝 시작
            _panStartPoint = e.GetPosition(this); // UserControl 기준
            WaferMapCanvas.CaptureMouse();
            WaferMapCanvas.Cursor = Cursors.Hand;
            e.Handled = true;
        }

        // ⭐️ 마우스 이동 (RMB가 눌렸을 때만 패닝)
        private void WaferMapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_panStartPoint.HasValue && e.RightButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(this);
                Vector delta = currentPoint - _panStartPoint.Value;
                _translateTransform.X += delta.X;
                _translateTransform.Y += delta.Y;
                _panStartPoint = currentPoint;
            }
        }

        // ⭐️ RMB UP = 패닝 종료
        private void WaferMapCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _panStartPoint = null;
            // ⭐️ 마우스 캡쳐 해제 조건 추가 (다른 버튼 누른 상태 아닐 때)
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

            if (!(DataContext is WaferViewModel viewModel) || viewModel.Dies == null || !viewModel.Dies.Any())
                return;

            // ⭐️ [수정] 고정 크기 대신 캔버스의 '실제' 크기 사용
            double canvasWidth = WaferMapCanvas.ActualWidth;
            double canvasHeight = WaferMapCanvas.ActualHeight;

            // 캔버스가 렌더링되기 전(크기가 0)이면 그리기 중단
            if (canvasWidth == 0 || canvasHeight == 0)
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

            // 0으로 나누기 방지
            if (dieActualWidth == 0) dieActualWidth = 1.0;
            if (dieActualHeight == 0) dieActualHeight = 1.0;
            // ⭐️ numRows, numCols 0 방지 추가
            if (numRows == 0) numRows = 1;
            if (numCols == 0) numCols = 1;


            // ⭐️ [수정] 캔버스 실제 크기에 맞게 렌더링 크기 계산
            double dieRenderWidth = canvasWidth / numRows * 0.95;
            double dieRenderHeight = canvasHeight / numCols * 0.95;

            // ⭐️ [수정] 가로/세로 비율을 유지하도록 렌더링 크기 재조정
            double aspectRatioActual = dieActualWidth / dieActualHeight;
            // ⭐️ 0으로 나누기 방지 추가
            if (dieRenderHeight == 0) dieRenderHeight = 0.001; // 아주 작은 값으로 대체
            double aspectRatioRender = dieRenderWidth / dieRenderHeight;


            if (aspectRatioRender > aspectRatioActual) // 렌더링이 실제보다 '넓적'하면
            {
                // ⭐️ 0으로 나누기 방지 추가
                // aspect ratio가 0이면 (폭이나 높이가 0이면) 비율 유지 불가능 -> 근사치 사용
                if (aspectRatioActual > 0)
                    dieRenderWidth = dieRenderHeight * aspectRatioActual; // 높이에 너비를 맞춤
                // else dieRenderWidth = dieRenderHeight; //비율이 0이면 정사각형으로 처리 (주석 처리 - 불필요)
            }
            else // 렌더링이 실제보다 '길쭉'하거나 비율이 같으면
            {
                // ⭐️ 0으로 나누기 방지 추가
                if (aspectRatioActual > 0)
                    dieRenderHeight = dieRenderWidth / aspectRatioActual; // 너비에 높이를 맞춤
                // else dieRenderHeight = dieRenderWidth; //비율이 0이면 정사각형으로 처리 (주석 처리 - 불필요)
            }


            string orientation = viewModel.Wafer?.Orientation?.ToUpper() ?? "DOWN";

            double totalMapWidth = numRows * (dieRenderWidth / 0.95);
            double totalMapHeight = numCols * (dieRenderHeight / 0.95);
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
                else // DOWN
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
                // ⭐️ NullReferenceException 방지
                if (die.Key != null)
                    _dieRectangles[die.Key] = rect;


                // 3. 해당 Die의 Defect 그리기
                // ⭐️ [수정] viewModel.AllDefects 사용 (사용자 코드)
                if (!die.IsGood && viewModel.AllDefects != null)
                {
                    var defectsInDie = viewModel.AllDefects.Where(def => def.Row == die.Row && def.Column == die.Column);
                    foreach (var defect in defectsInDie)
                    {
                        // ⭐️ 0으로 나누기 방지
                        if (dieActualWidth == 0 || dieActualHeight == 0) continue;

                        // --- ✨ Defect 좌표 계산 로직 (사용자 코드) ---
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
                        // --- ✨ 수정 끝 ---

                        var defectMarker = new Ellipse
                        {
                            // ⭐️ [수정] 점 크기를 4에서 2로 변경
                            Width = 2,
                            Height = 2,
                            Fill = Brushes.Yellow,
                            // ⭐️ [수정] 줌 배율에 관계없이 크기 고정
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

