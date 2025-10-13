using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlarfApplication.Model;
namespace KlarfApplication.Model
{
    public class FileModel : ModelBase
    {
        // 파일인지 디렉터리인지 확인할 수 있는 구조
        public readonly bool _isFile;
        public readonly string _fileName;
        public readonly string _fileExtension { get; }

    }
}
