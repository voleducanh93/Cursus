using NUnit.Framework;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Cursus.Repository.Repository;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using System.Linq.Expressions;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class AdminRepositoryTests
    {
        private AdminRepository _repository;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<CursusDbContext> _mockDbContext;

        [SetUp]
        public void Setup()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            _mockDbContext = new Mock<CursusDbContext>();

            _repository = new AdminRepository(_mockDbContext.Object, _mockUserManager.Object);
        }

        [Test]
        public async Task ToggleUserStatusAsync_ShouldThrowKeyNotFoundException_WhenUserNotFound()
        {
            // Arrange
            string userId = "nonexistent";
            _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _repository.ToggleUserStatusAsync(userId));
        }

        [Test]
        public async Task ToggleUserStatusAsync_ShouldUpdateStatus_WhenUserFound()
        {
            // Arrange
            string userId = "existing";
            var user = new ApplicationUser { Id = userId, Status = true };
            _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockUserManager.Setup(um => um.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _repository.ToggleUserStatusAsync(userId);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(user.Status, Is.False);
        }

        [Test]
        public async Task AdminComments_ShouldThrowKeyNotFoundException_WhenUserNotFound()
        {
            // Arrange
            string userId = "nonexistent";
            _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _repository.AdminComments(userId, "Test Comment"));
        }

        [Test]
        public async Task AdminComments_ShouldAddComment_WhenUserFound()
        {
            // Arrange
            string userId = "existing";
            var user = new ApplicationUser { Id = userId };
            var commentText = "Test Comment";
            _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);

            var dbSetMock = new Mock<DbSet<AdminComment>>();
            _mockDbContext.Setup(db => db.AdminComments).Returns(dbSetMock.Object);
            _mockDbContext.Setup(db => db.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _repository.AdminComments(userId, commentText);

            // Assert
            Assert.That(result, Is.True);
            dbSetMock.Verify(db => db.AddAsync(It.IsAny<AdminComment>(), default), Times.Once);
        }
    }
}
