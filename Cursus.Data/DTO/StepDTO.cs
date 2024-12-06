
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Cursus.Data.DTO
{
	public class StepDTO
	{
        public int Id { get; set; }
		public int CourseId { get; set; }
		public string Name { get; set; }
		public int Order { get; set; }
		public string Description { get; set; }
		public DateTime DateCreated { get; set; }
	}
}
