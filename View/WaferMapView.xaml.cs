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

            // 1. Wafer 원형 배경 그리기
            DrawWaferCircle();

            // 2. Die 좌표 범위 계산
            var minX = viewModel.Dies.Min(d => d.Row);
            var maxX = viewModel.Dies.Max(d => d.Row);
            var minY = viewModel.Dies.Min(d => d.Column);
            var maxY = viewModel.Dies.Max(d => d.Column);

            var rangeX = maxX - minX + 1;
            var rangeY = maxY - minY + 1;

            // 3. 캔버스 크기에 맞게 스케일 계산
            double scale = Math.Min(CANVAS_SIZE / (rangeX * viewModel.Wafer.DieWidth),
                                    CANVAS_SIZE / (rangeY * viewModel.Wafer.DieHeight)) * 0.9;

            double offsetX = CANVAS_SIZE / 2;
            double offsetY = CANVAS_SIZE / 2;

            // 4. 각 Die를 사각형으로 그리기
            foreach (var die in viewModel.Dies)
            {
                double x = (die.Row - minX) * die.Width * scale - (rangeX * die.Width * scale / 2) + offsetX;
                double y = (die.Column - minY) * die.Height * scale - (rangeY * die.Height * scale / 2) + offsetY;

                // Die 타입에 따라 색상 결정
                Brush fillBrush;
                if (die.IsEdge)
                {
                    fillBrush = Brushes.DarkGray;
                }
                else if (die.IsGood)
                {
                    fillBrush = new SolidColorBrush(Color.FromRgb(0x22, 0xAB, 0x28));
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

        private void DrawWaferCircle()
        {
            var circle = new Ellipse
            {
                Width = CANVAS_SIZE * 0.95,
                Height = CANVAS_SIZE * 0.95,
                Fill = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x30)),
                Stroke = Brushes.Gray,
                StrokeThickness = 2
            };

            Canvas.SetLeft(circle, CANVAS_SIZE * 0.025);
            Canvas.SetTop(circle, CANVAS_SIZE * 0.025);

            WaferMapCanvas.Children.Add(circle);
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