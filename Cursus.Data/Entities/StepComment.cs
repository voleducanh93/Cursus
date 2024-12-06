using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class StepComment
    {
        public int Id { get; set; }

        [ForeignKey("Step")]
        public int StepId { get; set; }

        public Step? Step { get; set; }
            
        public string Content { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; }


        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

    }
}
