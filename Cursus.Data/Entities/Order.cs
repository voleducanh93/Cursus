using System.ComponentModel.DataAnnotations;
using Cursus.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cursus.Data.Entities
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        [ForeignKey("Cart")]
        public int CartId { get; set; }
        public Cart? Cart { get; set; }
        public double Amount { get; set; }
        public string? discountCode { get; set; }
        public double discountAmount { get; set; }
        public double PaidAmount { get; set; }
        public DateTime DateCreated { get; set; }
        public OrderStatus Status { get; set; }

        [ForeignKey("Transaction")]
        public int TransactionId { get; set; }
        public Transaction? Transaction { get; set; }
        
    }
    public enum OrderStatus
    {
        PendingPayment,
        Paid,
        Failed
    }

}

