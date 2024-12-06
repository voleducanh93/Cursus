using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class Step
    {
        public int Id { get; set; }
        
        [ForeignKey("Course")]
        public int CourseId { get; set; }
        
        public Course? Course { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public int Order { get; set; }
        
        public string Description { get; set; } = string.Empty;
        
        public DateTime DateCreated { get; set; }

        public ICollection<StepComment> StepComments { get; set; } = new List<StepComment>();
    }
}
