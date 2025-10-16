using KlarfApplication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlarfApplication.Service
{
    public class DefectService
    {
        public IEnumerable<Defect> FilterByClass(IEnumerable<Defect> defects, string className)
            => defects.Where(d => d.DefectType == className);

        public IEnumerable<Defect> GetByDie(IEnumerable<Defect> defects, int row, int column)
            => defects.Where(d => d.Row == row && d.Column == column);
    }
}
