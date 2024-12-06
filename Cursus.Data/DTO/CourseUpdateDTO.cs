using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class CourseUpdateDTO
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int CategoryId { get; set; }
		[Required]
		public bool Status { get; set; }
		[Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public double Price { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Discount must be greater than or equal to 0.")]
        public int Discount { get; set; }
		[Required]
		public DateTime StartedDate { get; set; }
    }
}
