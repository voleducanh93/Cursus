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
    public class WalletRepositoryTests
    {
        private WalletRepositoy _walletRepository;
        private DbContextOptions<CursusDbContext> _dbContextOptions;
        private CursusDbContext _dbContext;

        [SetUp]
        public void Setup()
        {
            _dbContextOptions = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: "CursusDbTest")
                .Options;
            _dbContext = new CursusDbContext(_dbContextOptions);
            _walletRepository = new WalletRepositoy(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task AddAsync_ShouldAddWallet()
        {
            // Arrange
            var wallet = new Wallet
            {
                UserId = "user1",
                Balance = 100,
                DateCreated = DateTime.UtcNow
            };

            // Act
            var result = await _walletRepository.AddAsync(wallet);
            await _dbContext.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(_dbContext.Wallets.Count(), Is.EqualTo(1));
            Assert.That(_dbContext.Wallets.First().UserId, Is.EqualTo(wallet.UserId));
        }

        [Test]
        public async Task DeleteAsync_ShouldDeleteWallet()
        {
            // Arrange
            var wallet = new Wallet
            {
                UserId = "user1",
                Balance = 100,
                DateCreated = DateTime.UtcNow
            };
            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();

            // Act
            await _walletRepository.DeleteAsync(wallet);
            await _dbContext.SaveChangesAsync();

            // Assert
            Assert.That(_dbContext.Wallets.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllWallets()
        {
            // Arrange
            var wallets = new List<Wallet>
            {
                new Wallet { UserId = "user1", Balance = 100, DateCreated = DateTime.UtcNow },
                new Wallet { UserId = "user2", Balance = 200, DateCreated = DateTime.UtcNow }
            };
            _dbContext.Wallets.AddRange(wallets);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _walletRepository.GetAllAsync();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnFilteredWallets()
        {
            // Arrange
            var wallets = new List<Wallet>
            {
                new Wallet { UserId = "user1", Balance = 100, DateCreated = DateTime.UtcNow },
                new Wallet { UserId = "user2", Balance = 200, DateCreated = DateTime.UtcNow }
            };
            _dbContext.Wallets.AddRange(wallets);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _walletRepository.GetAllAsync(w => w.Balance > 150);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().UserId, Is.EqualTo("user2"));
        }

        [Test]
        public async Task GetAsync_ShouldReturnCorrectWallet()
        {
            // Arrange
            var wallet = new Wallet
            {
                UserId = "user1",
                Balance = 100,
                DateCreated = DateTime.UtcNow
            };
            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _walletRepository.GetAsync(w => w.UserId == "user1");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.UserId, Is.EqualTo("user1"));
        }

        [Test]
        public async Task GetAsync_ShouldReturnNullIfNotFound()
        {
            // Act
            var result = await _walletRepository.GetAsync(w => w.UserId == "user999");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateWallet()
        {
            // Arrange
            var wallet = new Wallet
            {
                UserId = "user1",
                Balance = 100,
                DateCreated = DateTime.UtcNow
            };
            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();

            wallet.Balance = 200;

            // Act
            await _walletRepository.UpdateAsync(wallet);
            await _dbContext.SaveChangesAsync();

            // Assert
            var updatedWallet = _dbContext.Wallets.First();
            Assert.That(updatedWallet.Balance, Is.EqualTo(200));
        }
    }
}
