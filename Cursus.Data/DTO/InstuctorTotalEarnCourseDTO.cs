using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO;

public class InstuctorTotalEarnCourseDTO
{
    public int Id { get; set; }
    public string InstructorName { get; set; }
    public double Earnings { get; set; }
    public int CourseCount { get; set; }
         
}
