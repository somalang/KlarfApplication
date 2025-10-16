using KlarfApplication.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlarfApplication.Service
{    // 파일 로드랑 저장만
    public class KlarfService
    {
        private readonly KlarfParser _parser = new KlarfParser();

        public KlarfModel LoadKlarf(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("KLARF 파일을 찾을 수 없습니다.", filePath);

            return _parser.Parse(filePath);
        }

        public void SaveKlarf(KlarfModel klarf, string path)
        {
            // KlarfModel → 텍스트 포맷으로 저장
        }
    }
}
