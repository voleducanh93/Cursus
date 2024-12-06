using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class StepCommentCreateDTO
    {
        public string? Content { get; set; }
        public int CourseId { get; set; }
        public int StepId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}