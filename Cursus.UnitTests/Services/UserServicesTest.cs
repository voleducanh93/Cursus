using NUnit.Framework;
using Moq;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Repository.Repository;
using System.Threading.Tasks;
using Cursus.Service.Services;
using Microsoft.AspNetCore.Http;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IMapper> _mockMapper;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private UserService _userService;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            var mockStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockStore.Object, null, null, null, null, null, null, null, null);

            _userService = new UserService(_mockUnitOfWork.Object, _mockMapper.Object, _mockUserManager.Object);
        }

        [Test]
        public async Task UpdateUserProfile_WhenUserExists_UpdatesSuccessfully()
        {
            // Arrange
            var userId = "testUserId";
            var updateDto = new UserProfileUpdateDTO
            {
                UserName = "newUsername",
                Email = "new@email.com",
                PhoneNumber = "1234567890",
                Address = "New Address"
            };

            var existingUser = new ApplicationUser
            {
                Id = userId,
                UserName = "oldUsername",
                Email = "old@email.com",
                PhoneNumber = "0987654321",
                Address = "Old Address",
                EmailConfirmed = true
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.ExiProfile(userId))
                .ReturnsAsync(existingUser);

            _mockUnitOfWork.Setup(u => u.UserRepository.UsernameExistsAsync(updateDto.UserName))
                .ReturnsAsync(false);

            _mockMapper.Setup(m => m.Map<UserProfileUpdateDTO>(It.IsAny<ApplicationUser>()))
                .Returns(updateDto);

            // Act
            var result = await _userService.UpdateUserProfile(userId, updateDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserName, Is.EqualTo(updateDto.UserName));
            _mockUnitOfWork.Verify(u => u.UserRepository.UpdProfile(It.IsAny<ApplicationUser>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Test]
        public async Task UpdateUserProfile_WhenEmailNotConfirmed_DoesNotUpdateEmail()
        {
            // Arrange
            var userId = "testUserId";
            var updateDto = new UserProfileUpdateDTO
            {
                UserName = "newUsername",
                Email = "new@email.com",
                PhoneNumber = "1234567890",
                Address = "New Address"
            };

            var existingUser = new ApplicationUser
            {
                Id = userId,
                UserName = "oldUsername",
                Email = "old@email.com",
                PhoneNumber = "0987654321",
                Address = "Old Address",
                EmailConfirmed = false  // Email is not confirmed
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.ExiProfile(userId))
                .ReturnsAsync(existingUser);

            _mockUnitOfWork.Setup(u => u.UserRepository.UsernameExistsAsync(updateDto.UserName))
                .ReturnsAsync(false);

            // Act
            await _userService.UpdateUserProfile(userId, updateDto);

            // Assert
            Assert.That(existingUser.Email, Is.EqualTo("old@email.com"));
        }

        [Test]
        public async Task UpdateUserProfile_WhenUserDoesNotExist_ThrowsException()
        {
            // Arrange
            var userId = "nonExistentUserId";
            var updateDto = new UserProfileUpdateDTO();

            _mockUnitOfWork.Setup(u => u.UserRepository.ExiProfile(userId))
                .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _userService.UpdateUserProfile(userId, updateDto));
            Assert.That(ex.Message, Is.EqualTo("User not found"));
        }

        [Test]
        public async Task UpdateUserProfile_WhenUsernameAlreadyExists_ThrowsException()
        {
            // Arrange
            var userId = "testUserId";
            var updateDto = new UserProfileUpdateDTO
            {
                UserName = "existingUsername"
            };

            var existingUser = new ApplicationUser
            {
                Id = userId,
                UserName = "differentUsername"
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.ExiProfile(userId))
                .ReturnsAsync(existingUser);

            _mockUnitOfWork.Setup(u => u.UserRepository.UsernameExistsAsync(updateDto.UserName))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () =>
                await _userService.UpdateUserProfile(userId, updateDto));
            Assert.That(ex.Message, Is.EqualTo("Username already exists"));
        }

        [Test]
        public async Task UpdateUserProfile_WhenSameUsername_DoesNotCheckUsernameExistence()
        {
            // Arrange
            var userId = "testUserId";
            var updateDto = new UserProfileUpdateDTO
            {
                UserName = "existingUsername"
            };

            var existingUser = new ApplicationUser
            {
                Id = userId,
                UserName = "existingUsername"  // Same username as in updateDto
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.ExiProfile(userId))
                .ReturnsAsync(existingUser);

            // Act
            await _userService.UpdateUserProfile(userId, updateDto);

            // Assert
            _mockUnitOfWork.Verify(u => u.UserRepository.UsernameExistsAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task UpdateUserProfile_WhenRepositoryUpdateFails_ThrowsException()
        {
            // Arrange
            var userId = "testUserId";
            var updateDto = new UserProfileUpdateDTO
            {
                UserName = "newUsername"
            };

            var existingUser = new ApplicationUser
            {
                Id = userId,
                UserName = "oldUsername"
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.ExiProfile(userId))
                .ReturnsAsync(existingUser);

            _mockUnitOfWork.Setup(u => u.UserRepository.UpdProfile(It.IsAny<ApplicationUser>()))
                .ThrowsAsync(new Exception("Database update failed"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _userService.UpdateUserProfile(userId, updateDto));
            Assert.That(ex.Message, Is.EqualTo("Database update failed"));
        }

        [Test]
        public async Task UpdateUserProfile_VerifyAllPropertiesUpdated()
        {
            // Arrange
            var userId = "testUserId";
            var updateDto = new UserProfileUpdateDTO
            {
                UserName = "newUsername",
                Email = "new@email.com",
                PhoneNumber = "1234567890",
                Address = "New Address"
            };

            var existingUser = new ApplicationUser
            {
                Id = userId,
                UserName = "oldUsername",
                Email = "old@email.com",
                PhoneNumber = "0987654321",
                Address = "Old Address",
                EmailConfirmed = true
            };

            ApplicationUser capturedUser = null;
            _mockUnitOfWork.Setup(u => u.UserRepository.ExiProfile(userId))
                .ReturnsAsync(existingUser);
            _mockUnitOfWork.Setup(u => u.UserRepository.UpdProfile(It.IsAny<ApplicationUser>()))
                .Callback<ApplicationUser>(u => capturedUser = u);

            // Act
            await _userService.UpdateUserProfile(userId, updateDto);

            // Assert
            Assert.That(capturedUser, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(capturedUser.UserName, Is.EqualTo(updateDto.UserName));
                Assert.That(capturedUser.Email, Is.EqualTo(updateDto.Email));
                Assert.That(capturedUser.PhoneNumber, Is.EqualTo(updateDto.PhoneNumber));
                Assert.That(capturedUser.Address, Is.EqualTo(updateDto.Address));
            });
        }

        [Test]
        public async Task CheckUserExistsAsync_WhenUserExists_ReturnsTrue()
        {
            // Arrange
            var userId = "existingUserId";
            var user = new ApplicationUser { Id = userId };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.CheckUserExistsAsync(userId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CheckUserExistsAsync_WhenUserDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var userId = "nonExistentUserId";

            _mockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _userService.CheckUserExistsAsync(userId);

            // Assert
            Assert.That(result, Is.False);
        }

    }
}