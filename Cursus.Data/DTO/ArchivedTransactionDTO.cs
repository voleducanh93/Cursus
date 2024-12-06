using Cursus.Data.Enums;

namespace Cursus.Data.DTO
{
    public class ArchivedTransactionDTO
    {
        public int TransactionId { get; set; }
        public int OriginalTransactionId { get; set; }
        public string? UserId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateArchived { get; set; }
        public double Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TransactionStatus Status { get; set; }
    }
}
