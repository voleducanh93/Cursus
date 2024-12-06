using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class StatisticsResponseDTO
    {
        public int TotalSales { get; set; }
        public double ChangePercentageSales { get; set; }
        public double TotalRevenues { get; set; }
        public double ChangePercentageRevenues { get;set; }
    }
}
