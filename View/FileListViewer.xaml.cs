using System;
using System.Windows;
using System.Windows.Controls;

namespace KlarfApplication.View
{
    public partial class FileListViewer : UserControl
    {
        public FileListViewer()
        {
            InitializeComponent();
        }

        private void FileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // 파일 선택 처리
        }
    }
}