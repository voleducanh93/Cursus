using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class InstructorStatisticDTO
    {
        public string InstructorId { get; set; }
        public string InstructorName { get; set; }
        public double TotalEarnings { get; set; }
        public long TotalCoursesSold { get; set; }
        public DateTime LastOrderDate { get; set; }
    }
}
