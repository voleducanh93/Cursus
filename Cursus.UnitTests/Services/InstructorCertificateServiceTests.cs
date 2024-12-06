using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class InstructorCertificateServiceTests
    {
        private Mock<IAzureBlobStorageService> _mockBlobStorageService;
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IInstructorCertificateRepository> _mockInstructorCertificateRepository;
        private Mock<IMapper> _mockMapper;
        private InstructorCertificateService _service;

        [SetUp]
        public void Setup()
        {
            _mockBlobStorageService = new Mock<IAzureBlobStorageService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockInstructorCertificateRepository = new Mock<IInstructorCertificateRepository>();
            _mockMapper = new Mock<IMapper>();

            // Setup UnitOfWork to return the mocked repository
            _mockUnitOfWork.Setup(u => u.InstructorCertificateRepository)
                .Returns(_mockInstructorCertificateRepository.Object);

            _service = new InstructorCertificateService(
                _mockBlobStorageService.Object,
                _mockUnitOfWork.Object,
                _mockInstructorCertificateRepository.Object,
                _mockMapper.Object
            );
        }

        [Test]
        public async Task UploadCertificatesAsync_ValidInput_SuccessfulUpload()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var instructorId = 1;
            var certificateType = "Professional";

            var mockFiles = new List<IFormFile>
            {
                CreateMockFormFile("test1.pdf", "application/pdf"),
                CreateMockFormFile("test2.pdf", "application/pdf")
            };

            // Setup GetInstructorIdByUserIdAsync
            _mockUnitOfWork.Setup(u => u.InstructorCertificateRepository.GetInstructorIdByUserIdAsync(userId))
                .ReturnsAsync(instructorId);

            // Setup blob storage upload
            _mockBlobStorageService.Setup(b => b.UploadFileAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync((IFormFile file) => $"https://example.com/{file.FileName}");

            // Setup mapping
            _mockMapper.Setup(m => m.Map<InstructorCertificate>(It.IsAny<InstructorCertificateDto>()))
                .Returns(new InstructorCertificate());

            // Act
            var result = await _service.UploadCertificatesAsync(userId, mockFiles, certificateType);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            _mockBlobStorageService.Verify(b => b.UploadFileAsync(It.IsAny<IFormFile>()), Times.Exactly(2));
            _mockInstructorCertificateRepository.Verify(r => r.AddAsync(It.IsAny<InstructorCertificate>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Test]
        public void UploadCertificatesAsync_InstructorNotFound_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var mockFiles = new List<IFormFile>
            {
                CreateMockFormFile("test1.pdf", "application/pdf")
            };

            // Setup GetInstructorIdByUserIdAsync to return null
            _mockUnitOfWork.Setup(u => u.InstructorCertificateRepository.GetInstructorIdByUserIdAsync(userId))
                .ReturnsAsync((int?)null);

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () =>
                await _service.UploadCertificatesAsync(userId, mockFiles, "Professional"),
                "Instructor not found for this user."
            );
        }

        [Test]
        public void UploadCertificatesAsync_NoFiles_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var instructorId = 1;

            // Setup GetInstructorIdByUserIdAsync
            _mockUnitOfWork.Setup(u => u.InstructorCertificateRepository.GetInstructorIdByUserIdAsync(userId))
                .ReturnsAsync(instructorId);

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () =>
                await _service.UploadCertificatesAsync(userId, new List<IFormFile>(), "Professional"),
                "Please upload at least one file."
            );
        }

        [Test]
        public async Task UploadCertificatesAsync_EmptyFiles_SkipsEmptyFiles()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var instructorId = 1;
            var certificateType = "Professional";

            var mockFiles = new List<IFormFile>
            {
                CreateMockEmptyFormFile("empty1.pdf", "application/pdf"),
                CreateMockFormFile("test2.pdf", "application/pdf")
            };

            // Setup GetInstructorIdByUserIdAsync
            _mockUnitOfWork.Setup(u => u.InstructorCertificateRepository.GetInstructorIdByUserIdAsync(userId))
                .ReturnsAsync(instructorId);

            // Setup blob storage upload
            _mockBlobStorageService.Setup(b => b.UploadFileAsync(It.Is<IFormFile>(f => f.Length > 0)))
                .ReturnsAsync((IFormFile file) => $"https://example.com/{file.FileName}");

            // Setup mapping
            _mockMapper.Setup(m => m.Map<InstructorCertificate>(It.IsAny<InstructorCertificateDto>()))
                .Returns(new InstructorCertificate());

            // Act
            var result = await _service.UploadCertificatesAsync(userId, mockFiles, certificateType);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            _mockBlobStorageService.Verify(b => b.UploadFileAsync(It.Is<IFormFile>(f => f.Length > 0)), Times.Once);
        }

        // Helper method to create mock IFormFile
        private IFormFile CreateMockFormFile(string fileName, string contentType)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.Length).Returns(1000); // Non-zero length
            fileMock.Setup(f => f.OpenReadStream()).Returns(new System.IO.MemoryStream(new byte[1000]));
            return fileMock.Object;
        }

        // Helper method to create mock empty IFormFile
        private IFormFile CreateMockEmptyFormFile(string fileName, string contentType)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.Length).Returns(0); // Zero length
            fileMock.Setup(f => f.OpenReadStream()).Returns(new System.IO.MemoryStream());
            return fileMock.Object;
        }
    }

    // DTO for mapping (you might want to place this in the appropriate file)
    public class InstructorCertificateDto
    {
        public int InstructorId { get; set; }
        public string CertificateType { get; set; }
        public List<string> FileUrls { get; set; }
    }
}