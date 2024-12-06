using Cursus.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cursus.Data.Entities
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TransactionId { get; set; }

        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public DateTime DateCreated { get; set; }

        public double? Amount { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;
        
        public TransactionStatus Status { get; set; }

        public string? Token { get; set; }

        public string Description { get; set; } = string.Empty;

        public List<TransactionHistory> TransactionHistories { get; set; } = new List<TransactionHistory>();


    }
    
}
