using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class MonthlyStatisticsDTO
    {
        public string Month { get; set; } 
        public int TotalSales { get; set; } 
        public double TotalRevenue { get; set; } 
        public double SalesGrowth { get; set; } 
        public double RevenueGrowth { get; set; } 
    }
}
