using Cursus.Data.DTO;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IArchivedTransactionService
    {
        public Task<ArchivedTransactionDTO> ArchiveTransaction(int transactionId);
        public Task<ArchivedTransactionDTO> GetArchivedTransaction(int transactionId);
        public Task<ArchivedTransactionDTO> UnarchiveTransaction(int transactionId);
        public Task<IEnumerable<ArchivedTransactionDTO>> GetAllArchivedTransactions();
    }
}
