using Cursus.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class Course
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }

        public bool Status { get; set; }

        public double Price { get; set; }

        public int Discount { get; set; }

        public DateTime StartedDate { get; set; }

        public double Rating { get; set; } = 0;
        [ForeignKey("InstructorInfo")]
        public int InstructorInfoId { get; set; }
        public ApproveStatus IsApprove { get; set; }

        public InstructorInfo? InstructorInfo { get; set; }

        public ICollection<CourseVersion> CourseVersions { get; set; } = new List<CourseVersion>();

        public ICollection<Step> Steps { get; set; } = new List<Step>();

        public ICollection<CourseComment> CourseComments { get; set; } = new List<CourseComment>();
    }

}
