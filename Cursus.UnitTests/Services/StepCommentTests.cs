using Cursus.API.Controllers;
using Cursus.Data.DTO;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class StepCommentTests
    {
        private Mock<IStepCommentService> _stepCommentServiceMock;
        private StepCommentController _controller;

        [SetUp]
        public void Setup()
        {
            _stepCommentServiceMock = new Mock<IStepCommentService>();
            _controller = new StepCommentController(_stepCommentServiceMock.Object);
        }

        [Test]
        public async Task PostComment_ShouldReturnOk_WhenCommentIsPosted()
        {
            // Arrange
            var dto = new StepCommentCreateDTO { Content = "Test Comment", CourseId = 1, StepId = 1, UserId = "user123" };
            var expectedComment = new StepCommentDTO { CommentId = 1, Content = "Test Comment", Username = "user123", DateCreated = DateTime.Now };

            _stepCommentServiceMock.Setup(s => s.PostStepComment(dto)).ReturnsAsync(expectedComment);

            // Act
            var result = await _controller.PostComment(dto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            //Assert.AreEqual(expectedComment, okResult.Value);
            Assert.That(expectedComment, Is.EqualTo(okResult.Value));
        }

        [Test]
        public async Task PostComment_ShouldThrowBadHttpRequestException_WhenDtoIsNull()
        {
            // Arrange
            StepCommentCreateDTO dto = null;

            // Act & Assert
            Assert.ThrowsAsync<BadHttpRequestException>(async () => await _controller.PostComment(dto));
        }

        [Test]
        public async Task GetComments_ShouldReturnOk_WhenCommentsExist()
        {
            // Arrange
            int stepId = 1;
            var comments = new List<StepCommentDTO>
            {
                new StepCommentDTO { CommentId = 1, Content = "Test Comment 1", Username = "user1", DateCreated = DateTime.Now },
                new StepCommentDTO { CommentId = 2, Content = "Test Comment 2", Username = "user2", DateCreated = DateTime.Now }
            };

            _stepCommentServiceMock.Setup(s => s.GetStepCommentsAsync(stepId)).ReturnsAsync(comments);

            // Act
            var result = await _controller.GetComments(stepId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            // Assert.AreEqual(comments, okResult.Value);
            Assert.That(comments, Is.EqualTo(okResult.Value));
        }

        [Test]
        public async Task DeleteComment_ShouldReturnNoContent_WhenCommentIsDeleted()
        {
            // Arrange
            int commentId = 1;
            string adminId = "admin123";

            _stepCommentServiceMock.Setup(s => s.DeleteStepCommentIfAdmin(commentId, adminId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteComment(commentId, adminId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());

        }

        [Test]
        public async Task DeleteComment_ShouldReturnNotFound_WhenCommentDoesNotExist()
        {
            // Arrange
            int commentId = 1;
            string adminId = "admin123";

            _stepCommentServiceMock.Setup(s => s.DeleteStepCommentIfAdmin(commentId, adminId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteComment(commentId, adminId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}
