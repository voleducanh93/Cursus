using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class CourseCommentCreateDTO
    {
        public string? Comment { get; set; }
        public int CourseId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public double Rating { get; set; }

    }
}
