using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class TrackingProgressDTO
    {
        public int Id { get; set; }
        public int CourseProgressId { get; set; }
        public int StepId { get; set; }
        public DateTime Date { get; set; }
    }

}
