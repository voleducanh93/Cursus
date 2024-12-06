using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class StepCommentDTO
    {
        public int CommentId { get; set; }

        public string Username { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
    }
}