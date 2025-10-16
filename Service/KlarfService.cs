using System;
using System.IO;
using KlarfApplication.Model;

namespace KlarfApplication.Service
{
    /// <summary>
    /// KLARF 파일의 로드 및 저장을 담당하는 서비스 클래스입니다.
    /// </summary>
    public class KlarfService
    {
        #region Fields

        private readonly KlarfParser _parser = new KlarfParser();

        #endregion

        #region Public Methods

        /// <summary>
        /// 지정된 경로에서 KLARF 파일을 불러와 KlarfModel 객체로 반환합니다.
        /// </summary>
        /// <param name="filePath">KLARF 파일 경로</param>
        /// <returns>파싱된 KlarfModel 객체</returns>
        public KlarfModel LoadKlarf(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("KLARF 파일을 찾을 수 없습니다.", filePath);
            }

            return _parser.Parse(filePath);
        }

        /// <summary>
        /// KlarfModel 데이터를 KLARF 파일 형식으로 저장합니다. (추후 구현 예정)
        /// </summary>
        /// <param name="klarf">저장할 KlarfModel 객체</param>
        /// <param name="path">저장할 파일 경로</param>
        public void SaveKlarf(KlarfModel klarf, string path)
        {
            // TODO: KlarfModel → 텍스트 포맷 변환 및 파일 저장 로직 구현
        }

        #endregion
    }
}
