using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;
using Moq;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.Repository.Repository;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class MessageRepositoryTests
    {
        private CursusDbContext _dbContext;
        private MessageRepository _messageRepository;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;

        [SetUp]
        public void Setup()
        {
            // Create options for in-memory database
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Create a mock UserManager
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Create the DbContext
            _dbContext = new CursusDbContext(options);

            // Create the repository
            _messageRepository = new MessageRepository(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        private ApplicationUser CreateTestUser(string id = "testuser1")
        {
            return new ApplicationUser
            {
                Id = id,
                UserName = $"{id}@test.com"
            };
        }

        [Test]
        public async Task AddAsync_ShouldAddMessageSuccessfully()
        {
            // Arrange
            var user = CreateTestUser();
            var message = new Message
            {
                Text = "Test Message",
                TimeStamp = DateTime.UtcNow,
                SenderId = user.Id,
                Sender = user,
                GroupName = "TestGroup"
            };

            // Act
            var addedMessage = await _messageRepository.AddAsync(message);
            await _dbContext.SaveChangesAsync();

            // Assert
            Assert.That(addedMessage, Is.Not.Null);
            Assert.That(addedMessage.Id, Is.GreaterThan(0));
            Assert.That(_dbContext.Messages.Any(m => m.Id == addedMessage.Id), Is.True);
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveMessageSuccessfully()
        {
            // Arrange
            var user = CreateTestUser();
            var message = new Message
            {
                Text = "Test Message",
                TimeStamp = DateTime.UtcNow,
                SenderId = user.Id,
                Sender = user,
                GroupName = "TestGroup"
            };
            await _dbContext.Messages.AddAsync(message);
            await _dbContext.SaveChangesAsync();

            // Act
            var deletedMessage = await _messageRepository.DeleteAsync(message);
            await _dbContext.SaveChangesAsync();

            // Assert
            Assert.That(deletedMessage, Is.Not.Null);
            Assert.That(_dbContext.Messages.Any(m => m.Id == message.Id), Is.False);
        }

        [Test]
        public async Task GetAllAsync_WithoutFilter_ShouldReturnAllMessages()
        {
            // Arrange
            var user = CreateTestUser();
            var messages = new List<Message>
            {
                new Message
                {
                    Text = "Message 1",
                    TimeStamp = DateTime.UtcNow,
                    SenderId = user.Id,
                    Sender = user,
                    GroupName = "Group1"
                },
                new Message
                {
                    Text = "Message 2",
                    TimeStamp = DateTime.UtcNow,
                    SenderId = user.Id,
                    Sender = user,
                    GroupName = "Group2"
                }
            };
            await _dbContext.Messages.AddRangeAsync(messages);
            await _dbContext.SaveChangesAsync();

            // Act
            var retrievedMessages = await _messageRepository.GetAllAsync();

            // Assert
            Assert.That(retrievedMessages, Is.Not.Null);
            Assert.That(retrievedMessages.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllAsync_WithFilter_ShouldReturnFilteredMessages()
        {
            // Arrange
            var user = CreateTestUser();
            var messages = new List<Message>
            {
                new Message
                {
                    Text = "Message 1",
                    TimeStamp = DateTime.UtcNow,
                    SenderId = user.Id,
                    Sender = user,
                    GroupName = "Group1"
                },
                new Message
                {
                    Text = "Message 2",
                    TimeStamp = DateTime.UtcNow,
                    SenderId = user.Id,
                    Sender = user,
                    GroupName = "Group2"
                }
            };
            await _dbContext.Messages.AddRangeAsync(messages);
            await _dbContext.SaveChangesAsync();

            // Act
            var retrievedMessages = await _messageRepository.GetAllAsync(
                filter: m => m.GroupName == "Group1");

            // Assert
            Assert.That(retrievedMessages, Is.Not.Null);
            Assert.That(retrievedMessages.Count(), Is.EqualTo(1));
            Assert.That(retrievedMessages.First().GroupName, Is.EqualTo("Group1"));
        }

        [Test]
        public async Task GetAsync_ShouldReturnSingleMessage()
        {
            // Arrange
            var user = CreateTestUser();
            var message = new Message
            {
                Text = "Test Message",
                TimeStamp = DateTime.UtcNow,
                SenderId = user.Id,
                Sender = user,
                GroupName = "TestGroup"
            };
            await _dbContext.Messages.AddAsync(message);
            await _dbContext.SaveChangesAsync();

            // Act
            var retrievedMessage = await _messageRepository.GetAsync(
                filter: m => m.Id == message.Id);

            // Assert
            Assert.That(retrievedMessage, Is.Not.Null);
            Assert.That(retrievedMessage.Id, Is.EqualTo(message.Id));
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateMessageSuccessfully()
        {
            // Arrange
            var user = CreateTestUser();
            var message = new Message
            {
                Text = "Original Message",
                TimeStamp = DateTime.UtcNow,
                SenderId = user.Id,
                Sender = user,
                GroupName = "TestGroup"
            };
            await _dbContext.Messages.AddAsync(message);
            await _dbContext.SaveChangesAsync();

            // Act
            message.Text = "Updated Message";
            var updatedMessage = await _messageRepository.UpdateAsync(message);
            await _dbContext.SaveChangesAsync();

            // Assert
            Assert.That(updatedMessage, Is.Not.Null);
            Assert.That(updatedMessage.Text, Is.EqualTo("Updated Message"));

            // Verify in database
            var dbMessage = await _dbContext.Messages.FindAsync(message.Id);
            Assert.That(dbMessage.Text, Is.EqualTo("Updated Message"));
        }

        [Test]
        public async Task GetAsync_WithIncludeProperties_ShouldIncludeSender()
        {
            // Arrange
            var user = CreateTestUser();
            var message = new Message
            {
                Text = "Test Message",
                TimeStamp = DateTime.UtcNow,
                SenderId = user.Id,
                Sender = user,
                GroupName = "TestGroup"
            };
            await _dbContext.Messages.AddAsync(message);
            await _dbContext.SaveChangesAsync();

            // Act
            var retrievedMessage = await _messageRepository.GetAsync(
                filter: m => m.Id == message.Id,
                includeProperties: "Sender");

            // Assert
            Assert.That(retrievedMessage, Is.Not.Null);
            Assert.That(retrievedMessage.Sender, Is.Not.Null);
            Assert.That(retrievedMessage.Sender.Id, Is.EqualTo(user.Id));
        }

        [Test]
        public async Task GetAllAsync_WithIncludeProperties_ShouldIncludeSender()
        {
            // Arrange
            var user = CreateTestUser();
            var messages = new List<Message>
            {
                new Message
                {
                    Text = "Message 1",
                    TimeStamp = DateTime.UtcNow,
                    SenderId = user.Id,
                    Sender = user,
                    GroupName = "Group1"
                }
            };
            await _dbContext.Messages.AddRangeAsync(messages);
            await _dbContext.SaveChangesAsync();

            // Act
            var retrievedMessages = await _messageRepository.GetAllAsync(
                includeProperties: "Sender");

            // Assert
            Assert.That(retrievedMessages, Is.Not.Null);
            Assert.That(retrievedMessages.First().Sender, Is.Not.Null);
            Assert.That(retrievedMessages.First().Sender.Id, Is.EqualTo(user.Id));
        }
    }
}