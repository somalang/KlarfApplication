using KlarfApplication.Model;
using KlarfApplication.Service;
using Microsoft.Win32; // ⭐️ CommonOpenFileDialog 대신 OpenFileDialog를 사용
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs; // (OpenFolder에서 여전히 사용)

namespace KlarfApplication.ViewModel
{
    /// <summary>
    /// 폴더 안의 파일을 트리구조로 보여주는 FileListViewer의 뷰모델
    /// </summary>
    public class FileListViewModel : ViewModelBase
    {
        #region Fields

        private readonly KlarfService _klarfService;
        private ObservableCollection<TreeNodeItem> _treeNodes;
        private KlarfModel _selectedFile;
        private Visibility _noFilesVisibility;
        private string _currentFolderPath;

        #endregion

        #region Properties

        public ObservableCollection<TreeNodeItem> TreeNodes
        {
            get => _treeNodes;
            set
            {
                _treeNodes = value;
                OnPropertyChanged(nameof(TreeNodes));
            }
        }

        public KlarfModel SelectedFile
        {
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                OnPropertyChanged(nameof(SelectedFile));
            }
        }

        public Visibility NoFilesVisibility
        {
            get => _noFilesVisibility;
            set
            {
                _noFilesVisibility = value;
                OnPropertyChanged(nameof(NoFilesVisibility));
            }
        }

        public string CurrentFolderPath
        {
            get => _currentFolderPath;
            set
            {
                _currentFolderPath = value;
                OnPropertyChanged(nameof(CurrentFolderPath));
            }
        }

        #endregion

        #region Commands

        public ICommand OpenFileCommand { get; }
        public ICommand OpenImageFolderCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearFilesCommand { get; }

        #endregion

        #region Constructors

        public FileListViewModel()
        {
            _klarfService = new KlarfService();
            TreeNodes = new ObservableCollection<TreeNodeItem>();
            NoFilesVisibility = Visibility.Visible;

            OpenFileCommand = new RelayCommand(OpenFolder);
            OpenImageFolderCommand = new RelayCommand(OpenImgFiles); // ⭐️ [수정] 메서드 이름 변경
            RefreshCommand = new RelayCommand(RefreshFiles);
            ClearFilesCommand = new RelayCommand(ClearFiles);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// (Klarf용) 폴더 선택하고 안의 파일들 있으면 트리 구조로 보여주기
        /// </summary>
        private void OpenFolder()
        {
            var dialog = new CommonOpenFileDialog
            {
                Title = "Select Folder",
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    string selectedFolder = dialog.FileName;

                    if (Directory.Exists(selectedFolder))
                    {
                        CurrentFolderPath = selectedFolder;
                        BuildFolderTree(selectedFolder);
                        NoFilesVisibility = TreeNodes.Any() ? Visibility.Collapsed : Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"폴더를 여는 중 오류가 발생했습니다.\n\n{ex.Message}",
                        "Folder Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// ⭐️ [수정] "폴더"가 아닌 "이미지 파일"들을 선택해서 트리로 구성합니다.
        /// </summary>
        private void OpenImgFiles()
        {
            // 1. 폴더 선택기(CommonOpenFileDialog) 대신 파일 선택기(OpenFileDialog)를 사용합니다.
            var dialog = new OpenFileDialog
            {
                Title = "Select Image(s)",
                Filter = "Image Files (*.jpg;*.png;*.tif;*.tiff)|*.jpg;*.png;*.tif;*.tiff|All files (*.*)|*.*",
                Multiselect = true // 2. 여러 파일 선택을 허용합니다.
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // 3. 기존 트리를 초기화합니다.
                    TreeNodes.Clear();
                    CurrentFolderPath = null; // 특정 폴더에 묶인 것이 아닙니다.

                    // 4. 선택된 파일들을 보여줄 부모 노드를 하나 만듭니다.
                    var rootNode = new TreeNodeItem
                    {
                        Header = "🖼️ Selected Images",
                        IsExpanded = true,
                        NodeType = TreeNodeType.Folder
                    };

                    // 5. dialog.FileNames는 선택된 "모든 파일의 경로 배열"입니다.
                    foreach (string filePath in dialog.FileNames)
                    {
                        string extension = Path.GetExtension(filePath).ToLower();

                        // 6. 각 파일을 트리 노드로 만듭니다.
                        var fileNode = new TreeNodeItem
                        {
                            Header = $"{GetFileIcon(extension)} {Path.GetFileName(filePath)}",
                            FullPath = filePath,
                            NodeType = GetFileType(extension)
                        };

                        // 7. 부모 노드의 자식으로 추가합니다.
                        rootNode.Children.Add(fileNode);
                    }

                    // 8. 완성된 부모 노드를 TreeView에 추가합니다.
                    TreeNodes.Add(rootNode);

                    NoFilesVisibility = TreeNodes.Any() ? Visibility.Collapsed : Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"이미지를 로드하는 중 오류가 발생했습니다.\n\n{ex.Message}",
                        "Image Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        /// <summary>
        /// 파일 리스트 모두 제거
        /// </summary>
        private void ClearFiles()
        {
            TreeNodes.Clear();
            CurrentFolderPath = null;
            NoFilesVisibility = Visibility.Visible;
            SelectedFile = null;
        }

        /// <summary>
        /// 파일 리스트를 새로 고칩니다.
        /// (Klarf 폴더가 열려있을 때만 작동)
        /// </summary>
        private void RefreshFiles()
        {
            if (!string.IsNullOrEmpty(CurrentFolderPath) && Directory.Exists(CurrentFolderPath))
            {
                BuildFolderTree(CurrentFolderPath);
                NoFilesVisibility = TreeNodes.Any() ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>
        /// 폴더 전체를 TreeView 구조로 구성합니다.
        /// </summary>
        private void BuildFolderTree(string folderPath)
        {
            TreeNodes.Clear();

            try
            {
                var rootNode = new TreeNodeItem
                {
                    Header = $"📁 {Path.GetFileName(folderPath)}",
                    FullPath = folderPath,
                    IsExpanded = true,
                    NodeType = TreeNodeType.Folder
                };

                LoadDirectoryContents(rootNode, folderPath);
                TreeNodes.Add(rootNode);
                NoFilesVisibility = TreeNodes.Any() && TreeNodes[0].Children.Any() ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폴더 구조를 로드하는 중 오류가 발생했습니다.\n\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 디렉토리의 모든 하위 폴더와 파일을 재귀적으로 로드합니다.
        /// </summary>
        private void LoadDirectoryContents(TreeNodeItem parentNode, string directoryPath)
        {
            try
            {
                // (이하 로직은 기존과 동일)
                var subDirectories = Directory.GetDirectories(directoryPath)
                    .OrderBy(d => Path.GetFileName(d));

                foreach (var subDir in subDirectories)
                {
                    var dirNode = new TreeNodeItem
                    {
                        Header = $"📁 {Path.GetFileName(subDir)}",
                        FullPath = subDir,
                        IsExpanded = false,
                        NodeType = TreeNodeType.Folder
                    };

                    LoadDirectoryContents(dirNode, subDir);
                    parentNode.Children.Add(dirNode);
                }

                var files = Directory.GetFiles(directoryPath)
                    .OrderBy(f => Path.GetFileName(f));

                foreach (var file in files)
                {
                    var extension = Path.GetExtension(file).ToLower();
                    string icon = GetFileIcon(extension);

                    var fileNode = new TreeNodeItem
                    {
                        Header = $"{icon} {Path.GetFileName(file)}",
                        FullPath = file,
                        NodeType = GetFileType(extension)
                    };

                    string[] klarfExtensions = { ".klarf", ".kla", ".klf", ".000", ".001", ".002" };

                    if (klarfExtensions.Contains(extension))
                    {
                        try
                        {
                            var klarf = _klarfService.LoadKlarf(file);
                            fileNode.Tag = klarf;

                            fileNode.Children.Add(new TreeNodeItem
                            {
                                Header = $"📋 Lot ID: {klarf.LotId}",
                                NodeType = TreeNodeType.Info
                            });
                            fileNode.Children.Add(new TreeNodeItem
                            {
                                Header = $"💿 Wafer ID: {klarf.WaferId}",
                                NodeType = TreeNodeType.Info
                            });
                            fileNode.Children.Add(new TreeNodeItem
                            {
                                Header = $"⚠️ Defects: {klarf.TotalDefectCount}",
                                NodeType = TreeNodeType.Info
                            });
                            fileNode.Children.Add(new TreeNodeItem
                            {
                                Header = $"🔲 Dies: {klarf.TotalDies}",
                                NodeType = TreeNodeType.Info
                            });
                            fileNode.NodeType = TreeNodeType.KlarfFile;
                        }
                        catch
                        {
                        }
                    }

                    parentNode.Children.Add(fileNode);
                }
            }
            catch (UnauthorizedAccessException)
            {
                parentNode.Children.Add(new TreeNodeItem
                {
                    Header = "🔒 Access Denied",
                    NodeType = TreeNodeType.Info
                });
            }
        }

        /// <summary>
        /// 파일 확장자에 따라 아이콘을 반환합니다.
        /// </summary>
        private string GetFileIcon(string extension)
        {
            return extension switch
            {
                ".klarf" => "📄",
                ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".tif" or ".tiff" => "🖼️",
                ".txt" or ".log" => "📝",
                ".xml" => "📋",
                ".csv" => "📊",
                ".zip" or ".rar" or ".7z" => "📦",
                ".exe" or ".dll" => "⚙️",
                _ => "📃"
            };
        }

        /// <summary>
        /// 파일 확장자에 따라 파일 타입을 반환합니다.
        /// </summary>
        private TreeNodeType GetFileType(string extension)
        {
            return extension switch
            {
                ".klarf" or ".kla" or ".klf" or ".000" or ".001" or ".002"
                    => TreeNodeType.KlarfFile,

                ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".tif" or ".tiff"
                    => TreeNodeType.ImageFile,

                _ => TreeNodeType.File
            };
        }

        #endregion
    }

    /// <summary>
    /// TreeView 아이템을 나타내는 클래스
    /// </summary>
    public class TreeNodeItem : ViewModelBase
    {
        private string _header;
        private bool _isExpanded;
        private object _tag;
        private string _fullPath;
        private TreeNodeType _nodeType;

        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                OnPropertyChanged(nameof(Header));
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        public object Tag
        {
            get => _tag;
            set
            {
                _tag = value;
                OnPropertyChanged(nameof(Tag));
            }
        }

        public string FullPath
        {
            get => _fullPath;
            set
            {
                _fullPath = value;
                OnPropertyChanged(nameof(FullPath));
            }
        }

        public TreeNodeType NodeType
        {
            get => _nodeType;
            set
            {
                _nodeType = value;
                OnPropertyChanged(nameof(NodeType));
            }
        }

        public ObservableCollection<TreeNodeItem> Children { get; set; } = new ObservableCollection<TreeNodeItem>();
    }

    /// <summary>
    /// 트리 노드 타입
    /// </summary>
    public enum TreeNodeType
    {
        Folder,
        File,
        KlarfFile,
        ImageFile,
        Info
    }
}