using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Linq.Expressions;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class RefreshTokenRepositoryTests
    {
        private CursusDbContext _context;
        private RefreshTokenRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: $"CursusDb_{Guid.NewGuid()}")
                .Options;

            _context = new CursusDbContext(options);
            _repository = new RefreshTokenRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddAsync_ShouldAddRefreshTokenToDatabase()
        {
            // Arrange
            var refreshToken = new RefreshToken
            {
                Token = "test-token",
                UserId = "user1",
                Expries = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };

            // Act
            var result = await _repository.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            // Assert
            var savedToken = await _context.Set<RefreshToken>().FirstOrDefaultAsync();
            Assert.That(savedToken, Is.Not.Null);
            Assert.That(savedToken.Token, Is.EqualTo(refreshToken.Token));
            Assert.That(savedToken.UserId, Is.EqualTo(refreshToken.UserId));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveRefreshTokenFromDatabase()
        {
            // Arrange
            var refreshToken = new RefreshToken
            {
                Token = "test-token",
                UserId = "user1",
                Expries = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };
            await _context.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(refreshToken);
            await _context.SaveChangesAsync();

            // Assert
            var deletedToken = await _context.Set<RefreshToken>().FirstOrDefaultAsync();
            Assert.That(deletedToken, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllRefreshTokens()
        {
            // Arrange
            var tokenList = new List<RefreshToken>
            {
                new RefreshToken { Token = "token1", UserId = "user1", Expries = DateTime.Now.AddDays(7), Created = DateTime.Now },
                new RefreshToken { Token = "token2", UserId = "user2", Expries = DateTime.Now.AddDays(7), Created = DateTime.Now }
            };
            await _context.AddRangeAsync(tokenList);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            CollectionAssert.AreEquivalent(tokenList, result);
        }

        [Test]
        public async Task GetRefreshTokenAsync_ShouldReturnToken()
        {
            // Arrange
            var refreshToken = new RefreshToken
            {
                Token = "test-token",
                UserId = "user1",
                Expries = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };
            await _context.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRefreshTokenAsync("test-token");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.EqualTo("test-token"));
        }

        [Test]
        public async Task GetRefreshTokenAsync_ShouldReturnNull_WhenTokenNotFound()
        {
            // Act
            var result = await _repository.GetRefreshTokenAsync("non-existent-token");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateRefreshTokenInDatabase()
        {
            // Arrange
            var refreshToken = new RefreshToken
            {
                Token = "test-token",
                UserId = "user1",
                Expries = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };
            await _context.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            // Act
            refreshToken.Revoked = DateTime.Now;
            await _repository.UpdateAsync(refreshToken);
            await _context.SaveChangesAsync();

            // Assert
            var updatedToken = await _context.Set<RefreshToken>().FirstOrDefaultAsync();
            Assert.That(updatedToken, Is.Not.Null);
            Assert.That(updatedToken.Revoked, Is.Not.Null);
        }

        [Test]
        public async Task GetAllAsync_WithFilter_ShouldReturnFilteredTokens()
        {
            // Arrange
            var tokenList = new List<RefreshToken>
            {
                new RefreshToken { Token = "token1", UserId = "user1", Expries = DateTime.Now.AddDays(7), Created = DateTime.Now },
                new RefreshToken { Token = "token2", UserId = "user2", Expries = DateTime.Now.AddDays(7), Created = DateTime.Now }
            };
            await _context.AddRangeAsync(tokenList);
            await _context.SaveChangesAsync();

            Expression<Func<RefreshToken, bool>> filter = x => x.UserId == "user1";

            // Act
            var result = await _repository.GetAllAsync(filter);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().UserId, Is.EqualTo("user1"));
        }

        [Test]
        public async Task GetAllAsync_WithIncludeProperties_ShouldIncludeRelatedEntities()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", UserName = "testuser" };
            var refreshToken = new RefreshToken
            {
                Token = "test-token",
                UserId = "user1",
                Expries = DateTime.Now.AddDays(7),
                Created = DateTime.Now,
                User = user
            };
            await _context.AddAsync(user);
            await _context.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync(includeProperties: "User");

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().User, Is.Not.Null);
            Assert.That(result.First().User.UserName, Is.EqualTo("testuser"));
        }

        [Test]
        public void RefreshToken_IsExpired_ShouldReturnTrue_WhenTokenIsExpired()
        {
            // Arrange
            var token = new RefreshToken
            {
                Token = "test-token",
                Expries = DateTime.Now.AddDays(-1)
            };

            // Assert
            Assert.That(token.IsExpried, Is.True);
        }

        [Test]
        public void RefreshToken_IsActive_ShouldReturnTrue_WhenTokenIsValid()
        {
            // Arrange
            var token = new RefreshToken
            {
                Token = "test-token",
                Expries = DateTime.Now.AddDays(1),
                Revoked = null
            };

            // Assert
            Assert.That(token.IsActive, Is.True);
        }

        [Test]
        public void RefreshToken_IsActive_ShouldReturnFalse_WhenTokenIsRevoked()
        {
            // Arrange
            var token = new RefreshToken
            {
                Token = "test-token",
                Expries = DateTime.Now.AddDays(1),
                Revoked = DateTime.Now
            };

            // Assert
            Assert.That(token.IsActive, Is.False);
        }

        [Test]
        public void RefreshToken_IsActive_ShouldReturnFalse_WhenTokenIsExpired()
        {
            // Arrange
            var token = new RefreshToken
            {
                Token = "test-token",
                Expries = DateTime.Now.AddDays(-1),
                Revoked = null
            };

            // Assert
            Assert.That(token.IsActive, Is.False);
        }
    }
}