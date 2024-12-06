using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Cursus.Data.Entities;
using Cursus.Data.DTO;
using Cursus.Repository.Repository;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using System.Linq.Expressions;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class ArchivedTransactionServiceTests
    {
        private ArchivedTransactionService _service;
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IMapper> _mockMapper;

        [SetUp]
        public void SetUp()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            _service = new ArchivedTransactionService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Test]
        public async Task ArchiveTransaction_ValidTransaction_ReturnsArchivedTransactionDTO()
        {
            // Arrange
            var transactionId = 1;
            var transaction = new Transaction { TransactionId = transactionId };
            var archivedTransaction = new ArchivedTransaction();

            _mockUnitOfWork.Setup(u => u.TransactionRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Transaction, bool>>>(), null))
                .ReturnsAsync(transaction);

            _mockMapper.Setup(m => m.Map<ArchivedTransaction>(transaction))
                .Returns(archivedTransaction);

            _mockUnitOfWork.Setup(u => u.TransactionRepository.DeleteAsync(transaction))
                .ReturnsAsync(transaction);

            _mockUnitOfWork.Setup(u => u.ArchivedTransactionRepository.AddAsync(archivedTransaction))
                .ReturnsAsync(archivedTransaction);

            _mockMapper.Setup(m => m.Map<ArchivedTransactionDTO>(archivedTransaction))
                .Returns(new ArchivedTransactionDTO());

            // Act
            var result = await _service.ArchiveTransaction(transactionId);

            // Assert
            Assert.That(result, Is.Not.Null);
            _mockUnitOfWork.Verify(u => u.TransactionRepository.DeleteAsync(transaction), Times.Once);
            _mockUnitOfWork.Verify(u => u.ArchivedTransactionRepository.AddAsync(archivedTransaction), Times.Once);
        }

        [Test]
        public void ArchiveTransaction_TransactionNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.TransactionRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Transaction, bool>>>(), null))
                .ReturnsAsync((Transaction)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ArchiveTransaction(1));
        }

        [Test]
        public async Task GetAllArchivedTransactions_ReturnsArchivedTransactions()
        {
            // Arrange
            var archivedTransactions = new List<ArchivedTransaction>
            {
                new ArchivedTransaction { Id = 1 },
                new ArchivedTransaction { Id = 2 }
            };

            _mockUnitOfWork.Setup(u => u.ArchivedTransactionRepository.GetAllAsync(It.IsAny<Expression<Func<ArchivedTransaction,bool>>>(),null))
                .ReturnsAsync(archivedTransactions);

            _mockMapper.Setup(m => m.Map<IEnumerable<ArchivedTransactionDTO>>(archivedTransactions))
                .Returns(new List<ArchivedTransactionDTO>
                {
                    new ArchivedTransactionDTO { TransactionId = 1 },
                    new ArchivedTransactionDTO { TransactionId = 2 }
                });

            // Act
            var result = await _service.GetAllArchivedTransactions();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetAllArchivedTransactions_NoTransactionsFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.ArchivedTransactionRepository.GetAllAsync(It.IsAny<Expression<Func<ArchivedTransaction, bool>>>(), null))
                .ReturnsAsync((IEnumerable<ArchivedTransaction>)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetAllArchivedTransactions());
        }

        [Test]
        public async Task GetArchivedTransaction_ValidTransactionId_ReturnsArchivedTransactionDTO()
        {
            // Arrange
            var archivedTransaction = new ArchivedTransaction { Id = 1 };
            _mockUnitOfWork.Setup(u => u.ArchivedTransactionRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ArchivedTransaction, bool>>>(), null))
                .ReturnsAsync(archivedTransaction);

            _mockMapper.Setup(m => m.Map<ArchivedTransactionDTO>(archivedTransaction))
                .Returns(new ArchivedTransactionDTO { TransactionId = 1 });

            // Act
            var result = await _service.GetArchivedTransaction(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TransactionId, Is.EqualTo(1));
        }

        [Test]
        public void GetArchivedTransaction_TransactionNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.ArchivedTransactionRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ArchivedTransaction, bool>>>(),null))
                .ReturnsAsync((ArchivedTransaction)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetArchivedTransaction(1));
        }

        [Test]
        public async Task UnarchiveTransaction_ValidTransaction_ReturnsArchivedTransactionDTO()
        {
            // Arrange
            var archivedTransaction = new ArchivedTransaction { Id = 1 };
            var transaction = new Transaction();

            _mockUnitOfWork.Setup(u => u.ArchivedTransactionRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ArchivedTransaction, bool>>>(), null))
                .ReturnsAsync(archivedTransaction);

            _mockMapper.Setup(m => m.Map<Transaction>(archivedTransaction))
                .Returns(transaction);

            _mockUnitOfWork.Setup(u => u.ArchivedTransactionRepository.DeleteAsync(archivedTransaction))
                .ReturnsAsync(archivedTransaction);

            _mockUnitOfWork.Setup(u => u.TransactionRepository.AddAsync(transaction))
                .ReturnsAsync(transaction);

            _mockMapper.Setup(m => m.Map<ArchivedTransactionDTO>(archivedTransaction))
                .Returns(new ArchivedTransactionDTO { TransactionId = 1 });

            // Act
            var result = await _service.UnarchiveTransaction(1);

            // Assert
            Assert.That(result,Is.Not.Null);
            Assert.That(result.TransactionId,Is.EqualTo(1));
            _mockUnitOfWork.Verify(u => u.ArchivedTransactionRepository.DeleteAsync(archivedTransaction), Times.Once);
            _mockUnitOfWork.Verify(u => u.TransactionRepository.AddAsync(transaction), Times.Once);
        }

        [Test]
        public void UnarchiveTransaction_TransactionNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.ArchivedTransactionRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ArchivedTransaction, bool>>>(), null))
                .ReturnsAsync((ArchivedTransaction)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UnarchiveTransaction(1));
        }
    }
}
