using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class WalletHistoryRepositoryTests
    {
        private WalletHistoryRepository _walletHistoryRepository;
        private DbContextOptions<CursusDbContext> _dbContextOptions;
        private CursusDbContext _dbContext;

        [SetUp]
        public void Setup()
        {
            _dbContextOptions = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: "CursusDbTest")
                .Options;
            _dbContext = new CursusDbContext(_dbContextOptions);
            _walletHistoryRepository = new WalletHistoryRepository(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task AnyAsync_ShouldReturnTrueIfRecordExists()
        {
            // Arrange
            var walletHistory = new WalletHistory
            {
                WalletId = 1,
                AmountChanged = 50,
                NewBalance = 150,
                Description = "Test Description"
            };
            _dbContext.WalletHistories.Add(walletHistory);
            await _dbContext.SaveChangesAsync();

            // Act
            var exists = await _walletHistoryRepository.AnyAsync(x => x.WalletId == walletHistory.WalletId);

            // Assert
            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task AnyAsync_ShouldReturnFalseIfRecordDoesNotExist()
        {
            // Act
            var exists = await _walletHistoryRepository.AnyAsync(x => x.WalletId == 999);

            // Assert
            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnCorrectRecord()
        {
            // Arrange
            var walletHistory = new WalletHistory
            {
                WalletId = 1,
                AmountChanged = 50,
                NewBalance = 150,
                Description = "Test Description"
            };
            _dbContext.WalletHistories.Add(walletHistory);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _walletHistoryRepository.GetByIdAsync(walletHistory.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Id, Is.EqualTo(walletHistory.Id));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnNullIfNotFound()
        {
            // Act
            var result = await _walletHistoryRepository.GetByIdAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByWalletId_ShouldReturnAllMatchingRecords()
        {
            // Arrange
            var walletHistories = new List<WalletHistory>
            {
                new WalletHistory { WalletId = 1, AmountChanged = 50, NewBalance = 150, Description = "Test 1" },
                new WalletHistory { WalletId = 1, AmountChanged = -20, NewBalance = 130, Description = "Test 2" },
                new WalletHistory { WalletId = 2, AmountChanged = 100, NewBalance = 200, Description = "Test 3" }
            };
            _dbContext.WalletHistories.AddRange(walletHistories);
            await _dbContext.SaveChangesAsync();

            // Act
            var results = await _walletHistoryRepository.GetByWalletId(1);

            // Assert
            Assert.That(results.Count(), Is.EqualTo(2));
            Assert.That(results.All(x => x.WalletId == 1), Is.True);
        }

        [Test]
        public async Task GetByWalletId_ShouldReturnEmptyIfNoRecordsMatch()
        {
            // Act
            var results = await _walletHistoryRepository.GetByWalletId(999);

            // Assert
            Assert.That(results, Is.Empty);
        }
    }
}
