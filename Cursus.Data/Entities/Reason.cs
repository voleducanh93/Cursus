using Cursus.Repository.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class Reason
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int CourseId { get; set; }

        public DateTime DateCancel { get; set; } = DateTime.UtcNow;

        public Course Course { get; set; }

        public int Status { get; set; } 
    }

}
