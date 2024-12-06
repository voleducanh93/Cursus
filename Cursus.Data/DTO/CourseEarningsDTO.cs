using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class CourseEarningsDTO
    {
        public string Status { get; set; }          
        public string ShortSummary { get; set; }   
        public double Earnings { get; set; }        
        public double PotentialEarnings { get; set; } 
        public double Price { get; set; }
    }
}
