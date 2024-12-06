using Cursus.Repository.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class CreateReasonDTO
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime DateCancel { get; set; } = DateTime.UtcNow;
        public ReasonStatus Status { get; set; } = ReasonStatus.Processing;
    }
}
