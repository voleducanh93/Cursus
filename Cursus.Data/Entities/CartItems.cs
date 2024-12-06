using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cursus.Data.Entities
{
	public class CartItems
	{
		[Key]
		public int CartItemsId { get; set; }
		public double Price { get; set; }

		public int CartId { get; set; }

		[ForeignKey("Course")]
		public int CourseId { get; set; }
		public Course Course { get; set; }
	}
}