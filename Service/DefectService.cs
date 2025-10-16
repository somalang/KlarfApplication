using System.Collections.Generic;
using System.Linq;
using KlarfApplication.Model;

namespace KlarfApplication.Service
{
    /// <summary>
    /// 결함(Defect) 데이터를 필터링하고 검색하는 서비스 클래스입니다.
    /// </summary>
    public class DefectService
    {
        #region Public Methods

        /// <summary>
        /// 지정된 결함 리스트에서 주어진 클래스명과 일치하는 결함만 필터링합니다.
        /// </summary>
        /// <param name="defects">전체 결함 목록</param>
        /// <param name="className">필터링할 결함 클래스 이름</param>
        /// <returns>해당 클래스의 결함 리스트</returns>
        public IEnumerable<Defect> FilterByClass(IEnumerable<Defect> defects, string className)
        {
            return defects.Where(defect => defect.DefectType == className);
        }

        /// <summary>
        /// 특정 다이(Row, Column)에 포함된 결함만 반환합니다.
        /// </summary>
        /// <param name="defects">전체 결함 목록</param>
        /// <param name="row">다이의 행 인덱스</param>
        /// <param name="column">다이의 열 인덱스</param>
        /// <returns>해당 다이에 속한 결함 리스트</returns>
        public IEnumerable<Defect> GetByDie(IEnumerable<Defect> defects, int row, int column)
        {
            return defects.Where(defect => defect.Row == row && defect.Column == column);
        }

        #endregion
    }
}
