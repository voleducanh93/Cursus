using Cursus.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cursus.Data.Entities
{
    public class ArchivedTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; }

        public ApplicationUser User { get; set; } = null!;

        public DateTime DateCreated { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public double Amount { get; set; }

        public string? Description { get; set; }

        public TransactionStatus Status { get; set; }

        public DateTime DateArchive { get; set; } = DateTime.Now;
    }
}
