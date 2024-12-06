using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class CourseResponseDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public DateTime DateCreated { get; set; }

        public bool Status { get; set; }

        public double Price { get; set; }

        public int Discount { get; set; }

        public DateTime StartedDate { get; set; }

        public double Rating { get; set; } = 0;
    }
}
