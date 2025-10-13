using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KlarfApplication.ViewModel
{
    public class FileListViewModel
    {
        //AddFileCommand()
        //RemoveFileCommand()
        public FileListViewModel() { }
        public FileListViewModel(string fileName) { }
        public ICommand OpenFileCommand { get; }
        public ICommand OpenImageFolderCommand { get; }
        public ICommand OpenAllCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand RefreshCommand{ get; }
        public ICommand ClearFilesCommand{ get; }
        public ICommand CloseFileCommand { get; }
        public ICommand CloseAllCommand { get; }

    }
}