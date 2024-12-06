using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Cursus.Data.DTO
{
	public class OrderDTO
	{
		public int OrderId { get; set; }
		public int CartId { get; set; }
		public double Amount { get; set; }
        public string? discountCode { get; set; }
        public double discountAmount { get; set; }
        public double PaidAmount { get; set; }
		public DateTime DateCreated { get; set; }
		public string Status { get; set; }
		public string TransactionId { get; set; }
	}
}
