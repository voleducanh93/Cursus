using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class CourseUpdateStatusDTO
    {
        public int Id { get; set; }
        public bool Status { get; set; }
    }
}
