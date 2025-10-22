using System;
using System.Windows;
using System.Windows.Media.Imaging; // ⭐️ 이 using 문을 추가합니다.

namespace KlarfApplication.View
{
    public partial class ImagePopupView : Window
    {
        public ImagePopupView(string imagePath)
        {
            InitializeComponent();

            try
            {
                // ⭐️ 생성자에서 파일 경로를 받아 이미지 소스로 설정
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // 파일 락 방지
                bitmap.EndInit();

                PopupImage.Source = bitmap;
                this.Title = imagePath; // 윈도우 제목을 파일 경로로 설정
            }
            catch (Exception ex)
            {
                MessageBox.Show($"이미지를 로드할 수 없습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }
    }
}