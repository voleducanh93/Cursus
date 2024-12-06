using Cursus.Data.Entities;
using Cursus.Data.Enums;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cursus.Repository.Repository
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        private readonly CursusDbContext _db;

        public TransactionRepository(CursusDbContext db) : base(db)
        {
            _db = db;
        }

        // Cập nhật trạng thái giao dịch
        public async Task UpdateTransactionStatus(int transactionId, TransactionStatus status)
        {
            var transaction = await _db.Transactions.FindAsync(transactionId);
            if (transaction != null)
            {
                transaction.Status = status;
               
            }
        }

       
        public async Task<IEnumerable<Transaction>> GetPendingTransactions()
        {
            return await _db.Transactions
                .Where(t => t.Status == TransactionStatus.Pending)
                .ToListAsync();
        }


        public async Task<Transaction?> GetPendingTransaction(int transactionId)
        {
            return await _db.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.Status == TransactionStatus.Pending);
        }

        public Task<bool> IsOrderCompleted(int transactionId)
        {
            return  _db.Transactions.AnyAsync(t => t.TransactionId == transactionId && t.Status == TransactionStatus.Completed);
        }

        public async Task<int> GetNextTransactionId()
        {
            var quantityTransaction = await _db.Transactions.CountAsync();

            var quantityArchivedTransaction = await _db.ArchivedTransactions.CountAsync();

            return quantityTransaction + quantityArchivedTransaction + 1;
        }
    }
}
