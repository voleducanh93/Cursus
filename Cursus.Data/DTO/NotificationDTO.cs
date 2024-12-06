namespace Cursus.Data.DTO
{
	public class NotificationDTO
	{
		public string Message { get; set; }
		public bool IsRead { get; set; }
		public bool IsNew { get; set; }
		public DateTime DateCreated { get; set; }
	}
}
