using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Cursus.ServiceContract.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class TransactionServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IUserService> _userServiceMock;
        private ITransactionService _transactionService;

        [SetUp]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _userServiceMock = new Mock<IUserService>();
            _transactionService = new TransactionService(_unitOfWorkMock.Object, _mapperMock.Object, _userServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _unitOfWorkMock = null;
            _mapperMock = null;
            _userServiceMock = null;
            _transactionService = null;
        }

        #region GetListTransaction Tests

        [Test]
        public async Task GetListTransaction_ShouldReturnPaginatedTransactionsSuccessfully()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, Status = Cursus.Data.Enums.TransactionStatus.Completed },
                new Transaction { TransactionId = 2, Status = Cursus.Data.Enums.TransactionStatus.Completed },
                 new Transaction { TransactionId = 3, Status = Cursus.Data.Enums.TransactionStatus.Failed }
            };

            _unitOfWorkMock.Setup(x => x.TransactionRepository.GetAllAsync(c => c.Status == Data.Enums.TransactionStatus.Completed, null))
                .ReturnsAsync(transactions);


            _mapperMock.Setup(m => m.Map<IEnumerable<TransactionDTO>>(It.IsAny<IEnumerable<Transaction>>()))
                       .Returns(new List<TransactionDTO>
                       {
                           new TransactionDTO { TransactionId = 1 },
                           new TransactionDTO { TransactionId = 2 }
                       });

            // Act
            var result = await _transactionService.GetListTransaction(1, 10);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetListTransaction_ShouldReturnEmptyList_WhenNoTransactionsFound()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.TransactionRepository.GetAllAsync(c => c.Status == Data.Enums.TransactionStatus.Completed, null))
                           .ReturnsAsync(new List<Transaction>());

            _mapperMock.Setup(m => m.Map<IEnumerable<TransactionDTO>>(It.IsAny<IEnumerable<Transaction>>()))
                       .Returns(new List<TransactionDTO>());

            // Act
            var result = await _transactionService.GetListTransaction(1, 10);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        #endregion

        #region GetListTransactionByUserId Tests

        [Test]
        public async Task GetListTransactionByUserId_ShouldReturnPaginatedTransactionsSuccessfully()
        {
            // Arrange
            string userId = "user1";
            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, Status = Cursus.Data.Enums.TransactionStatus.Completed, User = new ApplicationUser { Id = userId } },
                new Transaction { TransactionId = 2, Status = Cursus.Data.Enums.TransactionStatus.Completed, User = new ApplicationUser { Id = userId } },
                 new Transaction { TransactionId = 3, Status = Cursus.Data.Enums.TransactionStatus.Failed, User = new ApplicationUser { Id = userId } }
            };

            _userServiceMock.Setup(x => x.CheckUserExistsAsync(userId)).ReturnsAsync(true);

            _unitOfWorkMock.Setup(x => x.TransactionRepository.GetAllAsync(c => c.Status == Data.Enums.TransactionStatus.Completed, null))
                           .ReturnsAsync(transactions);

            _mapperMock.Setup(m => m.Map<IEnumerable<TransactionDTO>>(It.IsAny<IEnumerable<Transaction>>()))
                       .Returns(new List<TransactionDTO>
                       {
                           new TransactionDTO { TransactionId = 1 },
                           new TransactionDTO { TransactionId = 2 }
                       });

            // Act
            var result = await _transactionService.GetListTransactionByUserId(userId, 1, 10);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetListTransactionByUserId_ShouldThrowKeyNotFoundException_WhenUserNotFound()
        {
            // Arrange
            string userId = "user1";
            _userServiceMock.Setup(x => x.CheckUserExistsAsync(userId)).ReturnsAsync(false);

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.GetListTransactionByUserId(userId, 1, 10));
            Assert.That(ex.Message, Is.EqualTo("User not found."));
        }

        [Test]
        public async Task GetListTransactionByUserId_ShouldReturnEmptyList_WhenNoTransactionsFound()
        {
            // Arrange
            string userId = "user1";
            _userServiceMock.Setup(x => x.CheckUserExistsAsync(userId)).ReturnsAsync(true);

            _unitOfWorkMock.Setup(x => x.TransactionRepository.GetAllAsync(c => c.Status == Data.Enums.TransactionStatus.Completed, null))
                           .ReturnsAsync(new List<Transaction>());

            _mapperMock.Setup(m => m.Map<IEnumerable<TransactionDTO>>(It.IsAny<IEnumerable<Transaction>>()))
                       .Returns(new List<TransactionDTO>());

            // Act
            var result = await _transactionService.GetListTransactionByUserId(userId, 1, 10);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        #endregion
    }
}
