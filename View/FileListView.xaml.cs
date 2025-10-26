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
            //DataContext = new FileListViewModel();
        }

        // ⭐️ [수정] 이벤트 핸들러 로직 단순화
        private void FileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // 뷰모델 가져오기
            var viewModel = DataContext as FileListViewModel;
            if (viewModel == null) return;

            // ⭐️ 선택된 노드를 ViewModel의 프로퍼티에 할당
            //    (모든 로직은 ViewModel이 담당)
            viewModel.SelectedNode = e.NewValue as TreeNodeItem;

            // 파일 경로 출력 (디버깅용)
            if (e.NewValue is TreeNodeItem node && !string.IsNullOrEmpty(node.FullPath))
            {
                System.Diagnostics.Debug.WriteLine($"Selected: {node.FullPath}");
            }
        }
    }
}