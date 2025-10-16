using KlarfApplication.ViewModel;
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
            DataContext = new FileListViewModel();
        }

        private void FileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // 파일 선택 처리
            if (e.NewValue is TreeNodeItem node)
            {
                var viewModel = DataContext as FileListViewModel;
                if (viewModel != null)
                {
                    // KLARF 파일이면 SelectedFile에 설정
                    if (node.Tag is KlarfApplication.Model.KlarfModel klarf)
                    {
                        viewModel.SelectedFile = klarf;
                    }

                    // 파일 경로 출력 (디버깅용)
                    if (!string.IsNullOrEmpty(node.FullPath))
                    {
                        System.Diagnostics.Debug.WriteLine($"Selected: {node.FullPath}");
                    }
                }
            }
        }
    }
}