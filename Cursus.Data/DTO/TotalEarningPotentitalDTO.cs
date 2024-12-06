using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class TotalEarningPotentitalDTO
    {
        public string CourseName { get; set; }
        public double InitialRevenue { get; set; }
        public List<double> ProjectedRevenues { get; set; }
    }
}
