using Cursus.Data.Entities;
using Cursus.Data.Enums;
using Cursus.Data.Models;
using Cursus.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class TransactionRepositoryTests
    {
        private TransactionRepository _transactionRepository;
        private DbContextOptions<CursusDbContext> _dbContextOptions;
        private CursusDbContext _dbContext;

        [SetUp]
        public void Setup()
        {
            _dbContextOptions = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: "CursusDbTest")
                .Options;
            _dbContext = new CursusDbContext(_dbContextOptions);
            _transactionRepository = new TransactionRepository(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task UpdateTransactionStatus_ShouldUpdateStatus()
        {
            // Arrange
            var transaction = new Transaction
            {
                TransactionId = 1,
                Status = TransactionStatus.Pending
            };
            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            // Act
            await _transactionRepository.UpdateTransactionStatus(transaction.TransactionId, TransactionStatus.Completed);
            await _dbContext.SaveChangesAsync();

            // Assert
            var updatedTransaction = await _dbContext.Transactions.FindAsync(transaction.TransactionId);
            Assert.That(updatedTransaction, Is.Not.Null);
            Assert.That(updatedTransaction?.Status, Is.EqualTo(TransactionStatus.Completed));
        }

        [Test]
        public async Task GetPendingTransactions_ShouldReturnPendingTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, Status = TransactionStatus.Pending },
                new Transaction { TransactionId = 2, Status = TransactionStatus.Completed },
                new Transaction { TransactionId = 3, Status = TransactionStatus.Pending }
            };
            _dbContext.Transactions.AddRange(transactions);
            await _dbContext.SaveChangesAsync();

            // Act
            var pendingTransactions = await _transactionRepository.GetPendingTransactions();

            // Assert
            Assert.That(pendingTransactions.Count(), Is.EqualTo(2));
            Assert.That(pendingTransactions.All(t => t.Status == TransactionStatus.Pending), Is.True);
        }

        [Test]
        public async Task GetPendingTransaction_ShouldReturnCorrectTransaction()
        {
            // Arrange
            var transaction = new Transaction
            {
                TransactionId = 1,
                Status = TransactionStatus.Pending
            };
            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _transactionRepository.GetPendingTransaction(transaction.TransactionId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.TransactionId, Is.EqualTo(transaction.TransactionId));
        }

        [Test]
        public async Task GetPendingTransaction_ShouldReturnNullIfNotPending()
        {
            // Arrange
            var transaction = new Transaction
            {
                TransactionId = 1,
                Status = TransactionStatus.Completed
            };
            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _transactionRepository.GetPendingTransaction(transaction.TransactionId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task IsOrderCompleted_ShouldReturnTrueIfCompleted()
        {
            // Arrange
            var transaction = new Transaction
            {
                TransactionId = 1,
                Status = TransactionStatus.Completed
            };
            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _transactionRepository.IsOrderCompleted(transaction.TransactionId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsOrderCompleted_ShouldReturnFalseIfNotCompleted()
        {
            // Arrange
            var transaction = new Transaction
            {
                TransactionId = 1,
                Status = TransactionStatus.Pending
            };
            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _transactionRepository.IsOrderCompleted(transaction.TransactionId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task GetNextTransactionId_ShouldReturnCorrectNextId()
        {
            // Arrange
            _dbContext.Transactions.Add(new Transaction { TransactionId = 1 });
            _dbContext.Transactions.Add(new Transaction { TransactionId = 2 });
            _dbContext.ArchivedTransactions.Add(new ArchivedTransaction { Id= 3 });
            await _dbContext.SaveChangesAsync();

            // Act
            var nextId = await _transactionRepository.GetNextTransactionId();

            // Assert
            Assert.That(nextId, Is.EqualTo(4));
        }
    }
}
