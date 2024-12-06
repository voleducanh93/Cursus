using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class TrackingProgress
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; } = null!;

        [ForeignKey("CourseProgress")]
        public int ProgressId { get; set; }
        public CourseProgress CourseProgress { get; set; } = null!;

        [ForeignKey("Step")]
        public int StepId { get; set; }
        public Step Step { get; set; } = null!;

        public DateTime Date { get; set; } = DateTime.Now;
    }

}
