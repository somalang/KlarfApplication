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

        private void FileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // 뷰모델 가져오기
            var viewModel = DataContext as FileListViewModel;
            if (viewModel == null) return;

            // 선택된 노드 가져오기
            if (e.NewValue is TreeNodeItem node)
            {
                // KLARF 파일이면 SelectedFile에 설정
                if (node.Tag is KlarfApplication.Model.KlarfModel klarf)
                {
                    viewModel.SelectedFile = klarf;
                }
                // KLARF 파일이 아닌 다른 '파일' 노드(이미지 파일 포함)를 클릭한 경우
                else if (node.NodeType == TreeNodeType.File || node.NodeType == TreeNodeType.ImageFile)
                {
                    // 경고창 표시
                    MessageBox.Show("선택한 파일은 KLARF 파일이 아닙니다.", "파일 형식 오류",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);

                    // 선택 클리어 (MainViewModel이 감지하여 다른 뷰들도 초기화)
                    //viewModel.SelectedFile = null;
                }
                // 폴더나 정보 노드를 클릭한 경우
                else
                {
                    // 선택 클리어
                    //viewModel.SelectedFile = null;
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