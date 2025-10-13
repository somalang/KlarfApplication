using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KlarfApplication.View
{
    public partial class DefectImageViewer : UserControl
    {
        public DefectImageViewer()
        {
            InitializeComponent();
        }

        private void ImageCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 줌 기능 구현
        }

        private void ImageCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 드래그 시작
        }

        private void ImageCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // 드래그 중
        }

        private void ImageCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 드래그 종료
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // 밝기 조정
        }

        private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // 대비 조정
        }
    }
}