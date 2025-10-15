using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlarfApplication.Model;
namespace KlarfApplication.Model
{
    public class KlarfModel : ModelBase
    {
        public readonly string _fileName;
        public readonly string _filePath; 
        public readonly DateTime _fileDate;

        public readonly string _fileExtension;

    }
}
