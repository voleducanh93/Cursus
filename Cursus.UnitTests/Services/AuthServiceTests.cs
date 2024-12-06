using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using System.Collections.Generic;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Cursus.Data.Entities;
using Cursus.Repository.Repository;
using Cursus.Service.Services;
using Cursus.Data.DTO;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IEmailService> _emailServiceMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IConfiguration> _configurationMock;
        private AuthService _authService;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _emailServiceMock = new Mock<IEmailService>();
            _mapperMock = new Mock<IMapper>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["Jwt:Key"]).Returns("ThisIsASecretKeyForJwtTokenValidation2024!");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
            _configurationMock.Setup(c => c["AppSettings:FrontendUrl"]).Returns("http://localhost");
            _configurationMock.Setup(c => c["TokenSettings:PasswordResetTokenLifespan"]).Returns("60");

            _authService = new AuthService(
                _userManagerMock.Object,
                _configurationMock.Object,
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _roleManagerMock.Object,
                _emailServiceMock.Object
            );
        }

        [Test]
        public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                UserId = "u1",
                Expries = DateTime.UtcNow.AddDays(7),//Refresh hết hạn sau 7 ngày
                Created = DateTime.UtcNow
            };
            var user = new ApplicationUser { Id = "123", Email = "test@example.com", UserName = "test@example.com", Status = true, EmailConfirmed = true,  };
            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(true);
            _userManagerMock.Setup(u=>u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            _unitOfWorkMock.Setup(u => u.RefreshTokenRepository.AddAsync(It.IsAny<RefreshToken>()));
            _mapperMock.Setup(m => m.Map<UserDTO>(It.IsAny<ApplicationUser>())).Returns(new UserDTO());

            var result = await _authService.LoginAsync(new LoginRequestDTO
            {
                Username = "test@example.com",
                Password = "password"
            });

            Assert.That(result,Is.Not.Null);
            Assert.That(result.Token,Is.Not.Null);
            Assert.That(result.RefreshToken, Is.Not.Null);
        }

        [Test]
        public void LoginAsync_WithInvalidCredentials_ThrowsException()
        {
            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

            Assert.ThrowsAsync<BadHttpRequestException>(() => _authService.LoginAsync(new LoginRequestDTO
            {
                Username = "invalid@example.com",
                Password = "password"
            }));
        }

        [Test]
        public async Task RegisterAsync_WithValidData_CreatesUser()
        {
            var dto = new UserRegisterDTO
            {
                UserName = "newuser@example.com",
                PhoneNumber = "1234567890",
                Password = "password",
                Role = "User"
            };
            _mapperMock.Setup(m => m.Map<ApplicationUser>(dto)).Returns(new ApplicationUser
            {
                Id = "123",
                Email = dto.UserName,
                UserName = dto.UserName,
                PhoneNumber = dto.PhoneNumber
            });
            ApplicationUser user = _mapperMock.Object.Map<ApplicationUser>(dto);
            _unitOfWorkMock.Setup(u => u.UserRepository.UsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _unitOfWorkMock.Setup(u => u.UserRepository.PhoneNumberExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            var result = await _authService.RegisterAsync(dto);

            Assert.That(result, Is.Not.Null);
            _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
        }

        [Test]
        public async Task ConfirmEmail_WithValidToken_ReturnsTrue()
        {
            var user = new ApplicationUser { Id = "123", Email = "test@example.com" };
            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            var result = await _authService.ConfirmEmail("test@example.com", "valid-token");

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task ForgetPassword_SendsResetEmail()
        {
            var user = new ApplicationUser { Email = "test@example.com", Status = true };
            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>())).ReturnsAsync("reset-token");

            var result = await _authService.ForgetPassword("test@example.com");

            Assert.That(result, Is.True);
            _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ResetPasswordAsync_WithValidToken_ResetsPassword()
        {
            var user = new ApplicationUser { Email = "test@example.com" };
            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            var result = await _authService.ResetPasswordAsync("test@example.com", "valid-token", "new-password");

            Assert.That(result, Is.True);
        }
    }
}
