using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Cursus.Data.Entities;
using Cursus.Repository.Repository;
using Cursus.Data.Models;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class PlatformWalletRepositoryTests
    {
        private PlatformWalletRepository _repository;
        private DbContextOptions<CursusDbContext> _options;
        private CursusDbContext _context;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new CursusDbContext(_options);
            _context.PlatformWallets.Add(new PlatformWallet { Id = 1, Balance = 100.0 });
            _context.SaveChanges();

            _repository = new PlatformWalletRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetPlatformWallet_ShouldReturnFirstPlatformWallet()
        {
            // Act
            var result = await _repository.GetPlatformWallet();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Balance, Is.EqualTo(100.0));
        }

        [Test]
        public async Task GetPlatformWallet_ShouldThrowException_WhenNoWalletsExist()
        {
            // Arrange
            _context.PlatformWallets.RemoveRange(_context.PlatformWallets);
            await _context.SaveChangesAsync();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _repository.GetPlatformWallet();
            });
        }
    }
}
