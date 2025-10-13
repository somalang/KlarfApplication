using System;
using System.Windows;
using System.Windows.Controls;

namespace KlarfApplication.View
{
    public partial class DefectInfoViewer : UserControl
    {
        public DefectInfoViewer()
        {
            InitializeComponent();
        }

        private void DefectDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Defect 선택 처리
        }
    }
}