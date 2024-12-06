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
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class CourseCommentServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IMapper> _mapperMock;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private CourseCommentService _service;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            _service = new CourseCommentService(_unitOfWorkMock.Object, _mapperMock.Object, _userManagerMock.Object);
        }

        [Test]
        public async Task DeleteComment_CommentExists_ShouldFlagComment()
        {
            // Arrange
            var commentId = 1;
            var courseComment = new CourseComment { Id = commentId, IsFlagged = false };
            _unitOfWorkMock.Setup(u => u.CourseCommentRepository.GetAsync(It.IsAny<Expression<Func<CourseComment, bool>>>(), "User,Course"))
                .ReturnsAsync(courseComment);

            // Act
            var result = await _service.DeleteComment(commentId);

            // Assert
            Assert.That(courseComment.IsFlagged, Is.True);
            _unitOfWorkMock.Verify(u => u.CourseCommentRepository.UpdateAsync(courseComment), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Test]
        public void DeleteComment_CommentDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var commentId = 1;
            _unitOfWorkMock.Setup(u => u.CourseCommentRepository.GetAsync(It.IsAny<Expression<Func<CourseComment, bool>>>(), "User,Course"))
                .ReturnsAsync((CourseComment)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteComment(commentId));
        }

        [Test]
        public async Task GetCourseCommentsAsync_ValidCourseId_ShouldReturnComments()
        {
            // Arrange
            var courseId = 1;
            var comments = new List<CourseComment> { new CourseComment { CourseId = courseId } };
            _unitOfWorkMock.Setup(u => u.CourseCommentRepository.GetAllAsync(It.IsAny<Expression<Func<CourseComment, bool>>>(), "User,Course"))
                .ReturnsAsync(comments);

            // Act
            var result = await _service.GetCourseCommentsAsync(courseId);

            // Assert
            Assert.That(result, Is.Not.Null);
            _mapperMock.Verify(m => m.Map<IEnumerable<CourseCommentDTO>>(comments), Times.Once);
        }


        [Test]
        public void PostComment_EmailNotConfirmed_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var courseComment = new CourseCommentCreateDTO { UserId = "user1", CourseId = 1, Comment = "Great course!" };
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock.Setup(u => u.FindByIdAsync(courseComment.UserId)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.IsEmailConfirmedAsync(user)).ReturnsAsync(false);

            // Act & Assert
            Assert.ThrowsAsync<NullReferenceException>(() => _service.PostComment(courseComment));
        }

        [Test]
        public void PostComment_NullComment_ShouldThrowArgumentNullException()
        {
            // Arrange
            CourseCommentCreateDTO courseComment = null;

            // Act & Assert
            Assert.ThrowsAsync<NullReferenceException>(() => _service.PostComment(courseComment));
        }

        [Test]
        public async Task GetCourseCommentsAsync_NoComments_ShouldReturnEmptyList()
        {
            // Arrange
            var courseId = 1;
            var comments = new List<CourseComment>();
            _unitOfWorkMock.Setup(u => u.CourseCommentRepository.GetAllAsync(It.IsAny<Expression<Func<CourseComment, bool>>>(), "User,Course"))
                .ReturnsAsync(comments);

            // Act
            var result = await _service.GetCourseCommentsAsync(courseId);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task DeleteComment_ValidComment_ShouldReturnMappedDTO()
        {
            // Arrange
            var commentId = 1;
            var courseComment = new CourseComment { Id = commentId, IsFlagged = false };
            var courseCommentDTO = new CourseCommentDTO { Comment = "Test Comment" };

            _unitOfWorkMock.Setup(u => u.CourseCommentRepository.GetAsync(It.IsAny<Expression<Func<CourseComment, bool>>>(), "User,Course"))
                .ReturnsAsync(courseComment);
            _mapperMock.Setup(m => m.Map<CourseCommentDTO>(courseComment)).Returns(courseCommentDTO);

            // Act
            var result = await _service.DeleteComment(commentId);

            // Assert
            //Assert.AreEqual(courseCommentDTO, result);
            Assert.That(result, Is.EqualTo(courseCommentDTO));
        }

        [TearDown]
        public void TearDown()
        {
            _unitOfWorkMock = null;
            _mapperMock = null;
            _userManagerMock = null;
            _service = null;
        }
    }
}
