// View/WaferMapView.xaml.cs
using KlarfApplication.Model;
using KlarfApplication.ViewModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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
            DrawWaferCircle(viewModel.Wafer);

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

                var rect = new Rectangle
                {
                    Width = die.Width * scale * 0.95,
                    Height = die.Height * scale * 0.95,
                    Fill = die.IsGood ? Brushes.LimeGreen : Brushes.Red,
                    Stroke = Brushes.White,
                    StrokeThickness = 0.5,
                    Tag = die // Die 정보 저장
                };

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);

                // 클릭 이벤트
                rect.MouseLeftButtonDown += Die_MouseLeftButtonDown;

                WaferMapCanvas.Children.Add(rect);
            }
        }

        private void DrawWaferCircle(WaferModel wafer)
        {
            if (wafer == null) return;

            var circle = new Ellipse
            {
                Width = CANVAS_SIZE * 0.95,
                Height = CANVAS_SIZE * 0.95,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Gray,
                StrokeThickness = 2
            };

            Canvas.SetLeft(circle, CANVAS_SIZE * 0.025);
            Canvas.SetTop(circle, CANVAS_SIZE * 0.025);

            WaferMapCanvas.Children.Add(circle);
        }

        private void Die_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle rect && rect.Tag is DieViewModel die)
            {
                MessageBox.Show($"Die [{die.Row}, {die.Column}]\nDefects: {die.DefectCount}",
                               "Die Information",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
            }
        }

        private void WaferMapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 캔버스 클릭 처리 (필요시)
        }
    }
}