using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class StepContentDTO
    {
        public int Id { get; set; }
        public int StepId { get; set; }
        public string ?ContentType { get; set; } 
        public string ContentURL { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public string Description { get; set; } = string.Empty;

    }

    public class StepContentCreateDTO
    {

        public int StepId { get; set; }

        public string ContentType { get; set; } = string.Empty;

        public string ContentURL { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public string Description { get; set; } = string.Empty;

    }

}
