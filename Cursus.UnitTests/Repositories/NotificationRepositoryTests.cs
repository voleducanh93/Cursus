using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Moq;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.Repository.Repository;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class NotificationRepositoryTests
    {
        private CursusDbContext _context;
        private NotificationRepository _repository;
        private DbContextOptions<CursusDbContext> _options;

        [SetUp]
        public void Setup()
        {
            // Create in-memory database options
            _options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: "TestNotificationDb")
                .Options;

            // Create a fresh context for each test
            _context = new CursusDbContext(_options);
            _repository = new NotificationRepository(_context);

            // Ensure database is clean before each test
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task AddAsync_ShouldAddNotificationToDatabase()
        {
            // Arrange
            var notification = new Notification
            {
                Message = "Test Notification",
                UserId = "user123"
            };

            // Act
            var addedNotification = await _repository.AddAsync(notification);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(addedNotification, Is.Not.Null);
            Assert.That(addedNotification.NotifyId, Is.GreaterThan(0));
            Assert.That(addedNotification.Message, Is.EqualTo("Test Notification"));
            Assert.That(addedNotification.IsRead, Is.False);
            Assert.That(addedNotification.IsNew, Is.True);
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveNotificationFromDatabase()
        {
            // Arrange
            var notification = new Notification
            {
                Message = "Delete Test",
                UserId = "user456"
            };
            await _repository.AddAsync(notification);
            await _context.SaveChangesAsync();

            // Act
            var deletedNotification = await _repository.DeleteAsync(notification);
            await _context.SaveChangesAsync();

            // Assert
            var remainingNotifications = await _repository.GetAllAsync();
            Assert.That(remainingNotifications, Is.Empty);
            Assert.That(deletedNotification, Is.EqualTo(notification));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllNotifications()
        {
            // Arrange
            var notifications = new[]
            {
                new Notification { Message = "Notification 1", UserId = "user1" },
                new Notification { Message = "Notification 2", UserId = "user2" }
            };
            await _repository.AddAsync(notifications[0]);
            await _repository.AddAsync(notifications[1]);
            await _context.SaveChangesAsync();

            // Act
            var retrievedNotifications = await _repository.GetAllAsync();

            // Assert
            Assert.That(retrievedNotifications.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllAsync_WithFilter_ShouldReturnFilteredNotifications()
        {
            // Arrange
            var notifications = new[]
            {
                new Notification { Message = "Notification 1", UserId = "user1", IsRead = false },
                new Notification { Message = "Notification 2", UserId = "user2", IsRead = true }
            };
            await _repository.AddAsync(notifications[0]);
            await _repository.AddAsync(notifications[1]);
            await _context.SaveChangesAsync();

            // Act
            var retrievedNotifications = await _repository.GetAllAsync(n => n.IsRead == false);

            // Assert
            Assert.That(retrievedNotifications.Count(), Is.EqualTo(1));
            Assert.That(retrievedNotifications.First().Message, Is.EqualTo("Notification 1"));
        }

        [Test]
        public async Task GetAsync_ShouldReturnSpecificNotification()
        {
            // Arrange
            var notification = new Notification
            {
                Message = "Specific Notification",
                UserId = "user789"
            };
            await _repository.AddAsync(notification);
            await _context.SaveChangesAsync();

            // Act
            var retrievedNotification = await _repository.GetAsync(n => n.UserId == "user789");

            // Assert
            Assert.That(retrievedNotification, Is.Not.Null);
            Assert.That(retrievedNotification.Message, Is.EqualTo("Specific Notification"));
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateNotificationInDatabase()
        {
            // Arrange
            var notification = new Notification
            {
                Message = "Original Message",
                UserId = "user101"
            };
            await _repository.AddAsync(notification);
            await _context.SaveChangesAsync();

            // Act
            notification.Message = "Updated Message";
            notification.IsRead = true;
            var updatedNotification = await _repository.UpdateAsync(notification);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(updatedNotification.Message, Is.EqualTo("Updated Message"));
            Assert.That(updatedNotification.IsRead, Is.True);
        }

        [Test]
        public async Task GetAsync_WithNonExistentFilter_ShouldReturnNull()
        {
            // Arrange
            var notification = new Notification
            {
                Message = "Test Notification",
                UserId = "user202"
            };
            await _repository.AddAsync(notification);
            await _context.SaveChangesAsync();

            // Act
            var retrievedNotification = await _repository.GetAsync(n => n.UserId == "nonexistent");

            // Assert
            Assert.That(retrievedNotification, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_WithIncludeProperties_ShouldIncludeRelatedEntities()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user303", UserName = "TestUser" };
            _context.Users.Add(user);

            var notification = new Notification
            {
                Message = "Notification with User",
                UserId = user.Id,
                User = user
            };
            await _repository.AddAsync(notification);
            await _context.SaveChangesAsync();

            // Act
            var retrievedNotifications = await _repository.GetAllAsync(includeProperties: "User");

            // Assert
            var retrievedNotification = retrievedNotifications.First();
            Assert.That(retrievedNotification.User, Is.Not.Null);
            Assert.That(retrievedNotification.User.UserName, Is.EqualTo("TestUser"));
        }
    }
}