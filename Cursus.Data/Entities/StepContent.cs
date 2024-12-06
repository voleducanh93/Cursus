    using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class StepContent
    {
        public int Id { get; set; }

        [ForeignKey("Step")]
        public int StepId { get; set; }

        public Step? Step { get; set; }

        public string ContentType { get; set; } = string.Empty;

        public string ContentURL { get; set; } = string.Empty;
        
        public DateTime DateCreated { get; set; }
        
        public string Description { get; set; } = string.Empty;

    }
}
