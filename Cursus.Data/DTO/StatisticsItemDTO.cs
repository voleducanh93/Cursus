using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class StatisticsCourseResponseDTO
    {
        public int TotalCourse { get; set; } 
        public int ActiveCourse { get; set; }
        public int InActiveCourse { get; set; }


    }
}
