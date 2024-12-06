using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class MessageServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IMapper> _mockMapper;
        private MessageService _messageService;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _messageService = new MessageService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        private List<Message> CreateTestMessages(string groupName)
        {
            return new List<Message>
            {
                new Message
                {
                    Id = 1,
                    Text = "Message 1",
                    GroupName = groupName,
                    TimeStamp = DateTime.UtcNow.AddMinutes(-2),
                    SenderId = "user1"
                },
                new Message
                {
                    Id = 2,
                    Text = "Message 2",
                    GroupName = groupName,
                    TimeStamp = DateTime.UtcNow.AddMinutes(-1),
                    SenderId = "user2"
                }
            };
        }

        private List<ApplicationUser> CreateTestUsers()
        {
            return new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", UserName = "testuser1" },
                new ApplicationUser { Id = "user2", UserName = "testuser2" }
            };
        }

        [Test]
        public async Task AddMessage_ValidMessage_ShouldAddAndSave()
        {
            // Arrange
            var message = new Message
            {
                Text = "Test Message",
                GroupName = "TestGroup",
                SenderId = "user1"
            };

            _mockUnitOfWork.Setup(x => x.MessageRepository.AddAsync(message))
                .ReturnsAsync(message);
            _mockUnitOfWork.Setup(x => x.SaveChanges())
                .Returns(Task.CompletedTask);

            // Act
            await _messageService.AddMessage(message);

            // Assert
            _mockUnitOfWork.Verify(x => x.MessageRepository.AddAsync(message), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChanges(), Times.Once);
        }

        [Test]
        public void AddMessage_EmptyMessage_ShouldThrowBadHttpRequestException()
        {
            // Arrange
            var message = new Message { Text = "" };

            // Act & Assert
            Assert.ThrowsAsync<BadHttpRequestException>(async () =>
                await _messageService.AddMessage(message));
        }

        [Test]
        public async Task GetMessage_ExistingGroup_ShouldReturnOrderedMessages()
        {
            // Arrange
            string groupName = "TestGroup";
            var messages = CreateTestMessages(groupName);
            var users = CreateTestUsers();

            // Setup mocks
            _mockUnitOfWork.Setup(x => x.MessageRepository.GetAllAsync(
                    It.Is<System.Linq.Expressions.Expression<Func<Message, bool>>>(
                        f => f.Compile()(messages[0]) && f.Compile()(messages[1])), null))
                .ReturnsAsync(messages);

            _mockUnitOfWork.Setup(x => x.UserRepository.GetAllAsync(
                    It.Is<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>(
                        f => f.Compile()(users[0]) && f.Compile()(users[1])), null))
                .ReturnsAsync(users);

            // Setup mapper
            var messageDtos = messages.Select(m => new MessageDTO
            {
                Id = m.Id,
                Text = m.Text
            }).ToList();

            _mockMapper.Setup(m => m.Map<MessageDTO>(It.IsAny<Message>()))
                .Returns<Message>(msg => messageDtos.First(dto => dto.Id == msg.Id));

            // Act
            var result = await _messageService.GetMessage(groupName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Username, Is.EqualTo("testuser1"));
            Assert.That(result[1].Username, Is.EqualTo("testuser2"));

            // Verify the messages are ordered by timestamp
            Assert.That(result[0].Id, Is.EqualTo(1));
            Assert.That(result[1].Id, Is.EqualTo(2));
        }

        [Test]
        public async Task GetMessage_NoMessagesInGroup_ShouldReturnEmptyList()
        {
            // Arrange
            string groupName = "EmptyGroup";

            _mockUnitOfWork.Setup(x => x.MessageRepository.GetAllAsync(
                    It.Is<System.Linq.Expressions.Expression<Func<Message, bool>>>(
                        f => f.Compile()(new Message { GroupName = groupName })),null))
                .ReturnsAsync(new List<Message>());

            _mockUnitOfWork.Setup(x => x.UserRepository.GetAllAsync(
                    It.Is<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>(
                        f => true),null))
                .ReturnsAsync(new List<ApplicationUser>());

            // Act
            var result = await _messageService.GetMessage(groupName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetMessage_MultipleUsersInGroup_ShouldMapCorrectUsernames()
        {
            // Arrange
            string groupName = "MultiUserGroup";
            var messages = new List<Message>
            {
                new Message
                {
                    Id = 1,
                    Text = "Message 1",
                    GroupName = groupName,
                    TimeStamp = DateTime.UtcNow.AddMinutes(-2),
                    SenderId = "user1"
                },
                new Message
                {
                    Id = 2,
                    Text = "Message 2",
                    GroupName = groupName,
                    TimeStamp = DateTime.UtcNow.AddMinutes(-1),
                    SenderId = "user2"
                },
                new Message
                {
                    Id = 3,
                    Text = "Message 3",
                    GroupName = groupName,
                    TimeStamp = DateTime.UtcNow,
                    SenderId = "user1"
                }
            };

            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", UserName = "JohnDoe" },
                new ApplicationUser { Id = "user2", UserName = "JaneDoe" }
            };

            // Setup mocks
            _mockUnitOfWork.Setup(x => x.MessageRepository.GetAllAsync(
                    It.Is<System.Linq.Expressions.Expression<Func<Message, bool>>>(
                        f => f.Compile()(messages[0]) &&
                             f.Compile()(messages[1]) &&
                             f.Compile()(messages[2])), null))
                .ReturnsAsync(messages);

            _mockUnitOfWork.Setup(x => x.UserRepository.GetAllAsync(
                    It.Is<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>(
                        f => f.Compile()(users[0]) && f.Compile()(users[1])), null))
                .ReturnsAsync(users);

            // Setup mapper
            var messageDtos = messages.Select(m => new MessageDTO
            {
                Id = m.Id,
                Text = m.Text
            }).ToList();

            _mockMapper.Setup(m => m.Map<MessageDTO>(It.IsAny<Message>()))
                .Returns<Message>(msg => messageDtos.First(dto => dto.Id == msg.Id));

            // Act
            var result = await _messageService.GetMessage(groupName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));

            // Check usernames
            Assert.That(result[0].Username, Is.EqualTo("JohnDoe"));
            Assert.That(result[1].Username, Is.EqualTo("JaneDoe"));
            Assert.That(result[2].Username, Is.EqualTo("JohnDoe"));

            // Verify ordered by timestamp
            Assert.That(result[0].Id, Is.EqualTo(1));
            Assert.That(result[1].Id, Is.EqualTo(2));
            Assert.That(result[2].Id, Is.EqualTo(3));
        }
    }

}