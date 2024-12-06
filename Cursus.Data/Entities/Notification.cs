using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cursus.Data.Entities
{
	public class Notification
    {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int NotifyId { get; set; }
		[ForeignKey("ApplicationUser")]
		public string? UserId { get; set; }
		public ApplicationUser? User { get; set; }
		public string Message { get; set; }
		public DateTime DateCreated { get; set; } = DateTime.Now; 
		public bool IsRead { get; set; } = false;
		public bool IsNew { get; set; } = true;
	}
}
