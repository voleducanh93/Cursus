using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class PayoutRequestDTO
    {
        [Required]
        public string InstructorId { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "The amount must be greater than zero.")]
        public double Amount { get; set; }
    }
}
