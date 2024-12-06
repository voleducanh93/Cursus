using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class InstructorServicesTests
    {
        private InstructorService _instructorService;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IInstructorInfoRepository> _instructorInfoRepositoryMock;
        private Mock<IMapper> _mapperMock;
        private Mock<ICourseRepository> _courseMock;
        private Mock<IEmailService> _mailRepositoryMock;
        private Mock<IWalletService> _walletRepositoryMock;

        [SetUp]
        public void Setup()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null
            );
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _instructorInfoRepositoryMock = new Mock<IInstructorInfoRepository>();
            _mapperMock = new Mock<IMapper>();
            _courseMock = new Mock<ICourseRepository>();
            _mailRepositoryMock = new Mock<IEmailService>();
            _walletRepositoryMock = new Mock<IWalletService>();
            _unitOfWorkMock.Setup(u => u.InstructorInfoRepository).Returns(_instructorInfoRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.CourseRepository).Returns(_courseMock.Object);

            _instructorService = new InstructorService(
                    _userManagerMock.Object,
                    _unitOfWorkMock.Object,
                    _mailRepositoryMock.Object,
                    _mapperMock.Object,
                    _walletRepositoryMock.Object
                );
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
        }

        //User application 
        private UserManager<ApplicationUser> GetUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
            var userValidators = new List<IUserValidator<ApplicationUser>> { new Mock<IUserValidator<ApplicationUser>>().Object };
            var passwordValidators = new List<IPasswordValidator<ApplicationUser>> { new Mock<IPasswordValidator<ApplicationUser>>().Object };
            var keyNormalizer = new Mock<ILookupNormalizer>();
            var errors = new Mock<IdentityErrorDescriber>();
            var services = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

            return new UserManager<ApplicationUser>(
                store.Object,
                options.Object,
                passwordHasher.Object,
                userValidators,
                passwordValidators,
                keyNormalizer.Object,
                errors.Object,
                services.Object,
                logger.Object
            );
        }

        [Test]
        public async Task GetInstructorCoursesAsync_NoCourses_ThrowsKeyNotFoundException()
        {
            int instructorId = 123;

            var instructorInfo = TestDataHelper.GetInstructorInfo();
            var emptyCourses = new List<Course>();

            _instructorInfoRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<InstructorInfo, bool>>>(), null))
                .ReturnsAsync(instructorInfo);
            _courseMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), null))
                .ReturnsAsync(emptyCourses);
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _instructorService.GetTotalAmountAsync(instructorId)
            );
        }
        [Test]
        public async Task GetInstructorCoursesAsync_InstructorNotFound_ThrowsKeyNotFoundException()
        {
            int instructorId = 999;
            _instructorInfoRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<InstructorInfo, bool>>>(), null))
                .ReturnsAsync((InstructorInfo)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _instructorService.GetTotalAmountAsync(instructorId)
            );
        }
        [Test]
        public async Task RegisterInstructorAsync_Success_ReturnsUser()
        {
            // Arrange
            var registerInstructorDTO = new RegisterInstructorDTO
            {
                UserName = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Phone = "123456789",
                Address = "123 Main St",
                CardName = "Test Card",
                CardProvider = "Visa",
                CardNumber = "1234567812345678",
                SubmitCertificate = "SampleCertificate"
            };

            var user = new ApplicationUser
            {
                UserName = registerInstructorDTO.UserName,
                Email = registerInstructorDTO.UserName,
                PhoneNumber = registerInstructorDTO.Phone,
                Address = registerInstructorDTO.Address
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Instructor"))
                .ReturnsAsync(IdentityResult.Success);

            _unitOfWorkMock.Setup(u => u.InstructorInfoRepository.AddAsync(It.IsAny<InstructorInfo>()))
                                    .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChanges()).Returns(Task.CompletedTask);

            var result = await _instructorService.InstructorAsync(registerInstructorDTO);
            Assert.That(result, Is.Not.Null);
           // Assert.AreEqual(registerInstructorDTO.UserName, result.Email);
           Assert.That(result.Email, Is.EqualTo(registerInstructorDTO.UserName));
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Instructor"), Times.Once);
        }

        [Test]
        public async Task RegisterInstructorAsync_UserCreationFails_ReturnsNull()
        {

            var registerInstructorDTO = new RegisterInstructorDTO
            {
                UserName = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Phone = "123456789",
                Address = "123 Main St",
                CardName = "Test Card",
                CardProvider = "Visa",
                CardNumber = "1234567812345678",
                SubmitCertificate = "SampleCertificate"
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed to create user" }));

            var result = await _instructorService.InstructorAsync(registerInstructorDTO);

            Assert.That(result,Is.Null);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Instructor"), Times.Never);
        }
        [Test]
        public async Task RegisterInstructorAsync_InvalidUsername_ReturnsNull()
        {
            var registerInstructorDTO = new RegisterInstructorDTO
            {
                UserName = "invalid-email",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Phone = "123456789",
                Address = "123 Main St",
                CardName = "Test Card",
                CardProvider = "Visa",
                CardNumber = "1234567812345678",
                SubmitCertificate = "SampleCertificate"
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid email format" }));
            var result = await _instructorService.InstructorAsync(registerInstructorDTO);

            Assert.That(result, Is.Null);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task RegisterInstructorAsync_InvalidPassword_ReturnsNull()
        {
            var registerInstructorDTO = new RegisterInstructorDTO
            {
                UserName = "test@example.com",
                Password = "pass",
                ConfirmPassword = "pass",
                Phone = "123456789",
                Address = "123 Main St",
                CardName = "Test Card",
                CardProvider = "Visa",
                CardNumber = "1234567812345678",
                SubmitCertificate = "SampleCertificate"
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid password format" }));
            var result = await _instructorService.InstructorAsync(registerInstructorDTO);

            Assert.That(result, Is.Null);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task RegisterInstructorAsync_PasswordsDoNotMatch_ReturnsNull()
        {
            // Arrange
            var registerInstructorDTO = new RegisterInstructorDTO
            {
                UserName = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "DifferentPassword123!",
                Phone = "123456789",
                Address = "123 Main St",
                CardName = "Test Card",
                CardProvider = "Visa",
                CardNumber = "1234567812345678",
                SubmitCertificate = "SampleCertificate"
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Passwords do not match" }));
            var result = await _instructorService.InstructorAsync(registerInstructorDTO);

            Assert.That(result, Is.Null);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void RegisterInstructorAsync_InvalidCardNumber_ThrowsValidationException()
        {
            var registerInstructorDTO = new RegisterInstructorDTO
            {
                UserName = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Phone = "123456789",
                Address = "123 Main St",
                CardName = "Test Card",
                CardProvider = "Visa",
                CardNumber = "1234",
                SubmitCertificate = "SampleCertificate"
            };
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(registerInstructorDTO);

            var isValid = Validator.TryValidateObject(registerInstructorDTO, context, validationResults, true);

            var cardNumberError = validationResults.FirstOrDefault(vr => vr.ErrorMessage.Contains("Card number must be exactly 16 digits"));
            Assert.That(cardNumberError, Is.Not.Null);
            Assert.That(cardNumberError.ErrorMessage, Does.Contain("Card number must be exactly 16 digits"));
        }
        public static class TestDataHelper
        {
            public static InstructorInfo GetInstructorInfo()
            {
                return new InstructorInfo
                {
                    Id = 1,
                    UserId = "test-instructor-id",
                    User = new ApplicationUser { UserName = "John Doe" },
                    TotalEarning = 300,
                    TotalWithdrawn = 50
                };
            }

            public static List<Course> GetCourses()
            {
                return new List<Course>
        {
            new Course { Id = 1, Name = "Course 1", Price = 100.00 },
            new Course { Id = 2, Name = "Course 2", Price = 200.00 }
        };
            }
        }
    }
}
