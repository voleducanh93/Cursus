using System;
using System.Linq;
using System.Threading.Tasks;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.Repository.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class UserRepositoryTests
    {
        private CursusDbContext _dbContext;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private UserRepository _userRepository;

        [SetUp]
        public void Setup()
        {
            // Create options for in-memory database
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Create DbContext
            _dbContext = new CursusDbContext(options);

            // Mock UserManager
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null
            );

            // Create UserRepository
            _userRepository = new UserRepository(_dbContext, _userManagerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task ExiProfile_ExistingUser_ReturnsUser()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "testuser",
                PhoneNumber = "1234567890"
            };
            await _dbContext.ApplicationUsers.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userRepository.ExiProfile(user.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That( result.Id,Is.EqualTo(user.Id));
        }

        [Test]
        public void ExiProfile_NonExistingUser_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _userRepository.ExiProfile("non-existing-id")
            );
        }

        [Test]
        public async Task PhoneNumberExistsAsync_ExistingPhoneNumber_ReturnsTrue()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                PhoneNumber = "1234567890"
            };
            await _dbContext.ApplicationUsers.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userRepository.PhoneNumberExistsAsync("1234567890");

            // Assert
            Assert.That(result,Is.True);
        }

        [Test]
        public async Task PhoneNumberExistsAsync_NonExistingPhoneNumber_ReturnsFalse()
        {
            // Act
            var result = await _userRepository.PhoneNumberExistsAsync("9876543210");

            // Assert
            Assert.That(result,Is.False);
        }

        [Test]
        public async Task UpdProfile_ExistingUser_UpdatesUser()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "testuser",
                Address = "Old Address"
            };
            await _dbContext.ApplicationUsers.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Modify user
            user.Address = "New Address";

            // Act
            var updatedUser = await _userRepository.UpdProfile(user);

            // Assert
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.Address, Is.EqualTo("New Address"));
        }

        [Test]
        public async Task UsernameExistsAsync_ExistingUsername_ReturnsTrue()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser"
            };
            await _dbContext.ApplicationUsers.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userRepository.UsernameExistsAsync("testuser");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task UsernameExistsAsync_NonExistingUsername_ReturnsFalse()
        {
            // Act
            var result = await _userRepository.UsernameExistsAsync("nonexistinguser");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task UpdProfile_NullUser_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _userRepository.UpdProfile(null)
            );
        }
    }
}