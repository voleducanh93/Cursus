using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Cursus.Data.Entities;
using Cursus.Data.DTO;
using Cursus.Repository.Repository;
using Cursus.ServiceContract.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class CertificateServiceTests
    {
        private CertificateService _certificateService;
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IMapper> _mockMapper;
        private Mock<IEmailService> _mockEmailService;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ILogger<CertificateService>> _mockLogger;

        [SetUp]
        public void SetUp()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockEmailService = new Mock<IEmailService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<CertificateService>>();

            _certificateService = new CertificateService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockEmailService.Object,
                _mockConfiguration.Object,
                _mockLogger.Object
            );
        }

        [Test]
        public async Task CreateCertificate_ValidInputs_CreatesCertificateAndReturnsPdfData()
        {
            // Arrange
            var courseId = 1;
            var userId = "user123";
            var course = new Course { Id = courseId, Name = "Test Course" };
            var user = new ApplicationUser { Id = userId, UserName = "user@test.com" };

            _mockUnitOfWork.Setup(u => u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(), null))
                .ReturnsAsync(course);
            _mockUnitOfWork.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null))
                .ReturnsAsync(user);
            _mockUnitOfWork.Setup(u => u.CertificateRepository.GetAsync(It.IsAny<Expression<Func<Certificate, bool>>>(), null))
                .ReturnsAsync((Certificate)null);  // Ensure no certificate exists for the user and course

            _mockMapper.Setup(m => m.Map<Certificate>(It.IsAny<Certificate>()))
                .Returns(new Certificate());

            var pdfData = new byte[] { 37 };

            // Act
            var result = await _certificateService.CreateCertificate(courseId, userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            _mockEmailService.Verify(e => e.SendEmailCertificateCompletion(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void CreateCertificate_UserOrCourseNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var courseId = 1;
            var userId = "user123";

            _mockUnitOfWork.Setup(u => u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(),null))
                .ReturnsAsync((Course)null);  // Course not found
            _mockUnitOfWork.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(),null))
                .ReturnsAsync((ApplicationUser)null);  // User not found

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _certificateService.CreateCertificate(courseId, userId));
        }

        [Test]
        public void CreateCertificate_CertificateAlreadyExists_ThrowsBadHttpRequestException()
        {
            // Arrange
            var courseId = 1;
            var userId = "user123";
            var existingCertificate = new Certificate();
            var user = new ApplicationUser();
            _mockUnitOfWork.Setup(u=>u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(), null)).ReturnsAsync(new Course());
            _mockUnitOfWork.Setup(u => u.CertificateRepository.GetAsync(It.IsAny<Expression<Func<Certificate, bool>>>(), null))
                .ReturnsAsync(existingCertificate);
            _mockUnitOfWork.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null)).ReturnsAsync(user);
            _mockUnitOfWork.Setup(u => u.CertificateRepository.AddAsync(It.IsAny<Certificate>())).ReturnsAsync(existingCertificate);

            // Act & Assert
            Assert.ThrowsAsync<BadHttpRequestException>(() => _certificateService.CreateCertificate(courseId, userId));
        }

        [Test]
        public async Task GetCertificatePdfByIdAsync_ValidCertificateId_ReturnsPdfData()
        {
            // Arrange
            var certificateId = 1;
            var certificate = new Certificate { Id = certificateId, PdfData = new byte[] { 1, 2, 3 } };

            _mockUnitOfWork.Setup(u => u.CertificateRepository.GetAsync(It.IsAny<Expression<Func<Certificate, bool>>>(), null))
                .ReturnsAsync(certificate);

            // Act
            var result = await _certificateService.GetCertificatePdfByIdAsync(certificateId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(certificate.PdfData));
        }

        [Test]
        public void GetCertificatePdfByIdAsync_CertificateNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var certificateId = 1;

            _mockUnitOfWork.Setup(u => u.CertificateRepository.GetAsync(It.IsAny<Expression<Func<Certificate, bool>>>(), null))
                .ReturnsAsync((Certificate)null);  // Certificate not found

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _certificateService.GetCertificatePdfByIdAsync(certificateId));
        }

        [Test]
        public async Task GetCertificatePdfByUserAndCourseAsync_ValidInputs_ReturnsPdfData()
        {
            // Arrange
            var userId = "user123";
            var courseId = 1;
            var certificate = new Certificate { UserId = userId, CourseId = courseId, PdfData = new byte[] { 1, 2, 3 } };

            _mockUnitOfWork.Setup(u => u.CertificateRepository.GetAsync(It.IsAny<Expression<Func<Certificate, bool>>>(), null))
                .ReturnsAsync(certificate);

            // Act  
            var result = await _certificateService.GetCertificatePdfByUserAndCourseAsync(courseId, userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(certificate.PdfData));
        }

        [Test]
        public void GetCertificatePdfByUserAndCourseAsync_CertificateNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = "user123";
            var courseId = 1;

            _mockUnitOfWork.Setup(u => u.CertificateRepository.GetAsync(It.IsAny<Expression<Func<Certificate, bool>>>(), null))
                .ReturnsAsync((Certificate)null);  // Certificate not found

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _certificateService.GetCertificatePdfByUserAndCourseAsync(courseId, userId));
        }

        [Test]
        public async Task ExportCertificatesToExcel_ReturnsExcelData()
        {
            // Arrange
            var user = new ApplicationUser();
            var certificates = new List<Certificate>
            {
                new Certificate { UserId = "user123", CourseId = 1, CreateDate = DateTime.Now },
                new Certificate { UserId = "user456", CourseId = 2, CreateDate = DateTime.Now }
            };
            _mockUnitOfWork.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null))
                .ReturnsAsync(user);
            _mockUnitOfWork.Setup(u => u.CertificateRepository.GetAllAsync(It.IsAny<Expression<Func<Certificate,bool>>>(),null))
                .ReturnsAsync(certificates);
            _mockUnitOfWork.Setup(u => u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(), null))
    .ReturnsAsync(new Course());

            // Act
            var result = await _certificateService.ExportCertificatesToExcel();

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ExportCertificatesToExcel_NoCertificates_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.CertificateRepository.GetAllAsync(It.IsAny<Expression<Func<Certificate, bool>>>(), null))
                .ReturnsAsync(new List<Certificate>());  // No certificates found

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _certificateService.ExportCertificatesToExcel());
        }

        [Test]
        public async Task ExportCertificatesUserToExcel_ValidUser_ReturnsExcelData()
        {
            // Arrange
            var userId = "user123";
            var certificates = new List<Certificate>
            {
                new Certificate { UserId = userId, CourseId = 1, CreateDate = DateTime.Now }
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null))
                .ReturnsAsync(new ApplicationUser { Id = userId });

            _mockUnitOfWork.Setup(u => u.CertificateRepository.GetAllAsync(It.IsAny<Expression<Func<Certificate, bool>>>(), null))
                .ReturnsAsync(certificates);
            _mockUnitOfWork.Setup(u => u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(), null))
    .ReturnsAsync(new Course());
            // Act
            var result = await _certificateService.ExportCertificatesUserToExcel(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ExportCertificatesUserToExcel_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = "user123";

            _mockUnitOfWork.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null))
                .ReturnsAsync((ApplicationUser)null);  // User not found

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _certificateService.ExportCertificatesUserToExcel(userId));
        }

        [Test]
        public void ExportCertificatesUserToExcel_NoCertificates_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = "user123";

            _mockUnitOfWork.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null))
                .ReturnsAsync(new ApplicationUser { Id = userId });

            _mockUnitOfWork.Setup(u => u.CertificateRepository.GetAllAsync(It.IsAny<Expression<Func<Certificate, bool>>>(), null))
                .ReturnsAsync(new List<Certificate>());  // No certificates for user

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _certificateService.ExportCertificatesUserToExcel(userId));
        }
    }
}
