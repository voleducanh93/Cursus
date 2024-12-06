using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class CourseCommentDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        [Range(0,5)]
        public double Rating { get; set; }
        public DateTime Date { get; set; }
    }
}
