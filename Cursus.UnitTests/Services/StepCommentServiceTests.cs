using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class StepCommentServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IMapper> _mapperMock;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private StepCommentService _stepCommentService;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _userManagerMock = MockUserManager<ApplicationUser>();
            _stepCommentService = new StepCommentService(_unitOfWorkMock.Object, _mapperMock.Object, _userManagerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _unitOfWorkMock = null;
            _mapperMock = null;
            _userManagerMock = null;
            _stepCommentService = null;
        }

        private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            return new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        }


        [Test]
        [Category("Business Logic Tests")]
        public async Task PostStepComment_UserNotFound_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var stepComment = new StepCommentCreateDTO { UserId = "user1" };
            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _stepCommentService.PostStepComment(stepComment));
        }

        [Test]
        [Category("Business Logic Tests")]
        public async Task PostStepComment_EmailNotConfirmed_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var stepComment = new StepCommentCreateDTO { UserId = "user1" };
            var user = new ApplicationUser { EmailConfirmed = false };
            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _stepCommentService.PostStepComment(stepComment));
        }


        [Test]
        [Category("Business Logic Tests")]
        public async Task GetStepCommentsAsync_ValidStepId_ReturnsComments()
        {
            // Arrange
            var stepId = 1;
            var stepComments = new List<StepComment> { new StepComment { Content = "Test Comment" } };
            var stepCommentDTOs = new List<StepCommentDTO> { new StepCommentDTO { Content = "Test Comment" } };

            _unitOfWorkMock.Setup(u => u.StepCommentRepository.GetCommentsByStepId(It.IsAny<int>())).ReturnsAsync(stepComments);
            _mapperMock.Setup(m => m.Map<IEnumerable<StepCommentDTO>>(It.IsAny<IEnumerable<StepComment>>())).Returns(stepCommentDTOs);

            // Act
            var result = await _stepCommentService.GetStepCommentsAsync(stepId);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(stepCommentDTOs.Count));
        }


        [Test]
        [Category("Input Validation Tests")]
        public void DeleteStepCommentIfAdmin_AdminIdIsNull_ThrowsUnauthorizedAccessException()
        {
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _stepCommentService.DeleteStepCommentIfAdmin(1, null));
        }

        [Test]
        [Category("Business Logic Tests")]
        public async Task DeleteStepCommentIfAdmin_AdminNotFound_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var adminId = "admin1";
            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _stepCommentService.DeleteStepCommentIfAdmin(1, adminId));
        }

        [Test]
        [Category("Business Logic Tests")]
        public async Task DeleteStepCommentIfAdmin_AdminNotInRole_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var adminId = "admin1";
            var admin = new ApplicationUser();
            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(admin);
            _userManagerMock.Setup(u => u.IsInRoleAsync(It.IsAny<ApplicationUser>(), "Admin")).ReturnsAsync(false);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _stepCommentService.DeleteStepCommentIfAdmin(1, adminId));
        }

    }
}
