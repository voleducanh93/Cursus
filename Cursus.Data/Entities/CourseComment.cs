using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;


namespace Cursus.Data.Entities
{
    public class CourseComment
    {
        public int Id { get; set; }
        
        public string Comment { get; set; } = string.Empty;
        
        public double Rating { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }

        public Course? Course { get; set; }
        
        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;

        public bool IsFlagged { get; set; } = false; 
    }
}
