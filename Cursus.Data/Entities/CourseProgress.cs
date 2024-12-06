using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class CourseProgress
    {
        [Key]
        public int ProgressId { get; set; }
        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public Course? Course { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }    
        public ApplicationUser? ApplicationUser { get; set; }

        public string Type { get; set; }
        public DateTime Date { get; set; }
        public bool IsCompleted { get; set; }

    }
}
