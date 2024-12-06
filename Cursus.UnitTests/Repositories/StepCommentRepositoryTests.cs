using NUnit.Framework;
using Moq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.Repository.Repository;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class StepCommentRepositoryTests
    {
        private CursusDbContext _context;
        private StepCommentRepository _repository;
        private DbContextOptions<CursusDbContext> _options;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: "TestCursusDb")
                .Options;
        }

        [SetUp]
        public void Setup()
        {
            _context = new CursusDbContext(_options);
            _repository = new StepCommentRepository(_context);
            SeedDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SeedDatabase()
        {
            var user1 = new ApplicationUser
            {
                Id = "user1",
                UserName = "TestUser1"
            };

            var user2 = new ApplicationUser
            {
                Id = "user2",
                UserName = "TestUser2"
            };

            var step1 = new Step
            {
                Id = 1,
                Name = "Test Step 1"
            };

            var step2 = new Step
            {
                Id = 2,
                Name = "Test Step 2"
            };

            var comments = new List<StepComment>
            {
                new StepComment
                {
                    Id = 1,
                    StepId = 1,
                    Content = "Comment 1",
                    DateCreated = DateTime.UtcNow,
                    UserId = "user1",
                    User = user1,
                    Step = step1
                },
                new StepComment
                {
                    Id = 2,
                    StepId = 1,
                    Content = "Comment 2",
                    DateCreated = DateTime.UtcNow,
                    UserId = "user2",
                    User = user2,
                    Step = step1
                },
                new StepComment
                {
                    Id = 3,
                    StepId = 2,
                    Content = "Comment 3",
                    DateCreated = DateTime.UtcNow,
                    UserId = "user1",
                    User = user1,
                    Step = step2
                }
            };

            _context.Users.AddRange(user1, user2);
            _context.Steps.AddRange(step1, step2);
            _context.StepComments.AddRange(comments);
            _context.SaveChanges();
        }

        [Test]
        public async Task GetCommentsByStepId_ReturnsCorrectComments()
        {
            // Act
            var result = await _repository.GetCommentsByStepId(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(c => c.StepId == 1), Is.True);
        }

        [Test]
        public async Task GetCommentsByStepId_ReturnsEmptyList_WhenNoComments()
        {
            // Act
            var result = await _repository.GetCommentsByStepId(999);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetCommentsByStepId_IncludesUserAndStep()
        {
            // Act
            var result = await _repository.GetCommentsByStepId(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.All(c => c.User != null), Is.True);
            Assert.That(result.All(c => c.Step != null), Is.True);
        }

        [Test]
        public async Task GetCommentsByStepId_ReturnsCorrectUserData()
        {
            // Act
            var result = await _repository.GetCommentsByStepId(1);
            var comment = result.First();

            // Assert
            Assert.That(comment.User, Is.Not.Null);
            Assert.That(comment.User.UserName, Is.EqualTo("TestUser1").Or.EqualTo("TestUser2"));
        }

        [Test]
        public async Task GetCommentsByStepId_ReturnsCommentsInCorrectOrder()
        {
            // Arrange
            var newComment = new StepComment
            {
                Id = 4,
                StepId = 1,
                Content = "Latest Comment",
                DateCreated = DateTime.UtcNow.AddDays(1),
                UserId = "user1",
                User = await _context.Users.FindAsync("user1"),
                Step = await _context.Steps.FindAsync(1)
            };
            _context.StepComments.Add(newComment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCommentsByStepId(1);

            // Assert
            Assert.That(result.Count, Is.EqualTo(3));
        }
    }
}