using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Cursus.ServiceContract.Interfaces;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.UnitTests.Services
{
	[TestFixture]
	public class NotificationServiceTests
	{
		private Mock<IUnitOfWork> _unitOfWorkMock;
		private Mock<IMapper> _mapperMock;
		private NotificationService _notificationService;

		[SetUp]
		public void SetUp()
		{
			_unitOfWorkMock = new Mock<IUnitOfWork>();
			_mapperMock = new Mock<IMapper>();
			_notificationService = new NotificationService(_unitOfWorkMock.Object, _mapperMock.Object);
		}

		[TearDown]
		public void TearDown()
		{
			_unitOfWorkMock = null;
			_mapperMock = null;
			_notificationService = null;
		}

		#region Happy Path Scenarios

		[Test]
		public async Task FetchNotificationsAsync_ReturnsNotifications_WhenUserExists()
		{
			// Arrange
			var userId = "user123";
			var notifications = new List<Notification>
			{
				new Notification { NotifyId = 1, UserId = userId, Message = "Notification 1", DateCreated = DateTime.Now },
				new Notification { NotifyId = 2, UserId = userId, Message = "Notification 2", DateCreated = DateTime.Now.AddMinutes(-10) }
			};

			var notificationDTOs = new List<NotificationDTO>
			{
				new NotificationDTO { Message = "Notification 1", DateCreated = DateTime.Now },
				new NotificationDTO { Message = "Notification 2", DateCreated = DateTime.Now.AddMinutes(-10) }
			};

			_unitOfWorkMock.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null)).ReturnsAsync(new ApplicationUser { Id = userId });
			_unitOfWorkMock.Setup(u => u.NotificationRepository.GetAllAsync(It.IsAny<Expression<Func<Notification, bool>>>(), null)).ReturnsAsync(notifications);
			_mapperMock.Setup(m => m.Map<IEnumerable<NotificationDTO>>(It.IsAny<IEnumerable<Notification>>())).Returns(notificationDTOs);

			// Act
			var result = await _notificationService.FetchNotificationsAsync(userId);

			// Assert
			Assert.That(result.Count(), Is.EqualTo(2));
			_unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
		}

		#endregion

		#region Edge Cases

		[Test]
		public async Task SendNotificationAsync_WhenUserDoesNotExist()
		{
			// Arrange
			var userId = "nonexistentUser";
			var message = "Test Notification";
			_unitOfWorkMock.Setup(u => u.NotificationRepository.AddAsync(It.IsAny<Notification>()));
			_unitOfWorkMock.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null)).ReturnsAsync((ApplicationUser)null);

			// Act & Assert
_unitOfWorkMock.Verify(u => u.NotificationRepository.AddAsync(It.IsAny<Notification>()), Times.Never);
        }

		[Test]
		public async Task FetchNotificationsAsync_WhenUserDoesNotExist()
		{
			// Arrange
			var userId = "nonexistentUser";
			var Notification = new Notification
			{
				 UserId = userId,
            };

			_unitOfWorkMock.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null)).ReturnsAsync((ApplicationUser)null);
            _unitOfWorkMock.Setup(u => u.NotificationRepository.GetAllAsync(n => n.UserId == userId, null)).ReturnsAsync((Enumerable.Empty<Notification>));
            // Act & Assert
            var result= await _notificationService.FetchNotificationsAsync(userId);
			Assert.That(result,Is.Empty);
        }

		#endregion

		#region Error Conditions

		[Test]
		public void SendNotificationAsync_ThrowsException_WhenRepositoryFails()
		{
			// Arrange
			var userId = "user123";
			var message = "Test Notification";
			var user = new ApplicationUser { Id = userId };

			_unitOfWorkMock.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null)).ReturnsAsync(user);
			_unitOfWorkMock.Setup(u => u.NotificationRepository.AddAsync(It.IsAny<Notification>())).ThrowsAsync(new Exception("Database error"));

			// Act & Assert
			Assert.ThrowsAsync<Exception>(async () => await _notificationService.SendNotificationAsync(userId, message));
		}

		#endregion

	}
}
