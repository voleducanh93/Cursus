using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class CourseVersion
    {
        public int Id { get; set; }
        
        [ForeignKey("Course")]
        public int CourseId { get; set; }
        
        public Course? Course { get; set; }
        
        public int Version { get; set; }
        
        public string ChangeSummary { get; set; } = string.Empty;
        
        public DateTime DateModified { get; set; }
        
        public string ModifiedBy { get; set; } = string.Empty;

    }
}
