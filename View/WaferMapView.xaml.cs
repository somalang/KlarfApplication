using System;
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
        private const double CANVAS_SIZE = 500;

        public WaferMapViewer()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is WaferViewModel viewModel)
            {
                viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(WaferViewModel.Dies))
                    {
                        DrawWaferMap();
                    }
                };
            }
        }

        private void DrawWaferMap()
        {
            WaferMapCanvas.Children.Clear();

            if (!(DataContext is WaferViewModel viewModel) || viewModel.Dies == null || !viewModel.Dies.Any())
                return;

            // 2. Die 좌표 범위 계산
            var minX = viewModel.Dies.Min(d => d.Row);
            var maxX = viewModel.Dies.Max(d => d.Row);
            var minY = viewModel.Dies.Min(d => d.Column);
            var maxY = viewModel.Dies.Max(d => d.Column);

            var rangeX = maxX - minX + 1;
            var rangeY = maxY - minY + 1;

            string orientation = viewModel.Wafer?.Orientation ?? "DOWN"; // 기본값을 "DOWN"으로 설정
            // 3. 캔버스 크기에 맞게 스케일 계산
            double scale = Math.Min(CANVAS_SIZE / (rangeX * viewModel.Wafer.DieWidth),
                                    CANVAS_SIZE / (rangeY * viewModel.Wafer.DieHeight)) * 0.9;

            double offsetX = CANVAS_SIZE / 2;
            double offsetY = CANVAS_SIZE / 2;

            // 4. 각 Die를 사각형으로 그리기
            foreach (var die in viewModel.Dies)
            {
                double x, y;

                // X 좌표 계산 (X축은 대부분의 방향에서 동일)
                double xPos = (die.Row - minX) * die.Width * scale - (rangeX * die.Width * scale / 2) + offsetX;

                // Y 좌표 계산 (Y축은 방향에 따라 반전됨)
                double yPos;

                // ⭐️ 방향에 따라 Y축 좌표를 다르게 계산합니다.
                // ⭐️ 방향에 따라 Y축 좌표를 다르게 계산합니다.
                switch (orientation.ToUpper())
                {
                    case "UP":
                        // Notch가 위쪽: KLARF 최소 Y가 캔버스 최소 Y(상단)에 매핑됩니다.
                        yPos = (die.Column - minY) * die.Height * scale - (rangeY * die.Height * scale / 2) + offsetY;
                        break;

                    case "DOWN":
                        // Notch가 아래쪽: KLARF 최소 Y가 캔버스 최대 Y(하단)에 매핑됩니다. (Y축 반전)
                        yPos = (maxY - die.Column) * die.Height * scale - (rangeY * die.Height * scale / 2) + offsetY;
                        break;

                    // 참고: "LEFT"와 "RIGHT"는 90도 회전이 필요합니다.

                    case "LEFT":
                    case "RIGHT":
                    default:
                        // 기본값으로 "DOWN" 방향의 로직을 사용합니다 (Y축 반전)
                        yPos = (maxY - die.Column) * die.Height * scale - (rangeY * die.Height * scale / 2) + offsetY;
                        break;
                }

                x = xPos;
                y = yPos;

                // Die 타입에 따라 색상 결정
                // (이전 수정 사항: IsGood = 회색, !IsGood = 빨간색)
                Brush fillBrush;
                if (die.IsGood)
                {
                    fillBrush = Brushes.DarkGray;
                }
                else
                {
                    fillBrush = new SolidColorBrush(Color.FromRgb(0xE3, 0x04, 0x13));
                }

                var rect = new Rectangle
                {
                    Width = die.Width * scale * 0.95,
                    Height = die.Height * scale * 0.95,
                    Fill = fillBrush,
                    Stroke = Brushes.White,
                    StrokeThickness = 0.5,
                    Tag = die
                };

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);

                rect.MouseLeftButtonDown += Die_Click;
                rect.MouseEnter += Die_MouseEnter;
                rect.MouseLeave += Die_MouseLeave;

                WaferMapCanvas.Children.Add(rect);
            }
        }

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

        private void Die_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle rect && rect.Tag is DieViewModel die)
            {
                string message = $"Die Position: [{die.Row}, {die.Column}]\n" +
                                $"Type: {die.DieType}\n" +
                                $"Defects: {die.DefectCount}";

                MessageBox.Show(message, "Die Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void WaferMapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 캔버스 클릭 처리
        }
    }
}