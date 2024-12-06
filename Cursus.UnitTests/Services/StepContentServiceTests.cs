using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class StepContentServiceTests
    {
        private Mock<IStepContentRepository> _mockStepContentRepository;
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IMapper> _mockMapper;
        private Mock<IWebHostEnvironment> _mockWebHostEnvironment;
        private StepContentService _stepContentService;

        [SetUp]
        public void SetUp()
        {
            _mockStepContentRepository = new Mock<IStepContentRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();

            _mockUnitOfWork.Setup(u => u.StepContentRepository).Returns(_mockStepContentRepository.Object);

            _stepContentService = new StepContentService(
                _mockStepContentRepository.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockWebHostEnvironment.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup if necessary
        }

        [Test]
        public async Task CreateStepContent_ValidInput_ShouldCreateStepContent()
        {
            // Arrange
            var stepContentDTO = new StepContentDTO { StepId = 1, ContentType = "Video" };
            var stepContent = new StepContent { StepId = 1, ContentType = "Video" };

            _mockMapper.Setup(m => m.Map<StepContent>(It.IsAny<StepContentDTO>())).Returns(stepContent);
            _mockStepContentRepository.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<StepContent, bool>>>())).ReturnsAsync((StepContent)null);
            _mockStepContentRepository.Setup(r => r.AddAsync(It.IsAny<StepContent>())).ReturnsAsync(stepContent);
            _mockMapper.Setup(m => m.Map<StepContentDTO>(It.IsAny<StepContent>())).Returns(stepContentDTO);

            // Act
            var result = await _stepContentService.CreateStepContent(stepContentDTO);

            // Assert
            Assert.That(result, Is.EqualTo(stepContentDTO));
            _mockStepContentRepository.Verify(r => r.AddAsync(It.IsAny<StepContent>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Test]
        public void CreateStepContent_NullContentType_ShouldThrowArgumentException()
        {
            // Arrange
            var stepContentDTO = new StepContentDTO { StepId = 1, ContentType = null };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() => _stepContentService.CreateStepContent(stepContentDTO));
            Assert.That(ex.Message, Is.EqualTo("ContentType is required."));
        }

        [Test]
        public void CreateStepContent_ExistingStepContent_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var stepContentDTO = new StepContentDTO { StepId = 1, ContentType = "Video" };
            var existingStepContent = new StepContent { StepId = 1, ContentType = "Video" };

            _mockStepContentRepository.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<StepContent, bool>>>())).ReturnsAsync(existingStepContent);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _stepContentService.CreateStepContent(stepContentDTO));
            Assert.That(ex.Message, Is.EqualTo("StepContent already exists for this Step."));
        }

        [Test]
        public async Task GetStepContentByIdAsync_ValidId_ShouldReturnStepContent()
        {
            // Arrange
            var stepContent = new StepContent { Id = 1, StepId = 1, ContentType = "Video" };
            var stepContentDTO = new StepContentDTO { Id = 1, StepId = 1, ContentType = "Video" };

            _mockStepContentRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(stepContent);
            _mockMapper.Setup(m => m.Map<StepContentDTO>(It.IsAny<StepContent>())).Returns(stepContentDTO);

            // Act
            var result = await _stepContentService.GetStepContentByIdAsync(1);

            // Assert
            Assert.That(result, Is.EqualTo(stepContentDTO));
        }

        [Test]
        public void GetStepContentByIdAsync_InvalidId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockStepContentRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((StepContent)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _stepContentService.GetStepContentByIdAsync(1));
            Assert.That(ex.Message, Is.EqualTo("Step Content not found."));
        }


        [Test]
        public void CreateStepContentWithFileAsync_NullFile_ShouldThrowArgumentException()
        {
            // Arrange
            var stepContentDTO = new StepContentDTO { StepId = 1, ContentType = "Video" };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() => _stepContentService.CreateStepContentWithFileAsync(stepContentDTO, null));
            Assert.That(ex.Message, Is.EqualTo("No file uploaded...."));
        }

        [Test]
        public void CreateStepContentWithFileAsync_EmptyFile_ShouldThrowArgumentException()
        {
            // Arrange
            var stepContentDTO = new StepContentDTO { StepId = 1, ContentType = "Video" };
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(_ => _.Length).Returns(0);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() => _stepContentService.CreateStepContentWithFileAsync(stepContentDTO, fileMock.Object));
            Assert.That(ex.Message, Is.EqualTo("No file uploaded...."));
        }
    }
}
