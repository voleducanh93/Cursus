using Cursus.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Cursus.Data.DTO
{
	public class CourseDTO
	{
		public int InstructorId { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int CategoryId { get; set; }
		public DateTime DateCreated { get; set; }

		public DateTime DateModified { get; set; }
		public bool Status { get; set; }
		public double Price { get; set; }
		public int Discount { get; set; }
		public DateTime StartedDate { get; set; }
		public List<StepDTO> Steps { get; set; }
        public double Rating { get; set; }

		public ApproveStatus IsApprove { get; set; }
	}
}
