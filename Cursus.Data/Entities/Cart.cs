using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cursus.Data.Entities
{
	public class Cart
	{
		[Key]
		public int CartId { get; set; }
		public Boolean IsPurchased { get; set; }
		public double Total { get; set; }
		[ForeignKey("ApplicationUser")]
		public string? UserId { get; set; }
		public ApplicationUser? User { get; set; }

		public ICollection<CartItems> CartItems { get; set; } = new List<CartItems>();

	}
}