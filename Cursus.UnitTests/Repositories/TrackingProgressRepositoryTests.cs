using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.Repository.Repository;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class TrackingProgressRepositoryTests
    {
        private CursusDbContext _context;
        private TrackingProgressRepository _repository;
        private DbContextOptions<CursusDbContext> _options;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: "TestTrackingProgressDb")
                .Options;
        }

        [SetUp]
        public void Setup()
        {
            _context = new CursusDbContext(_options);
            _repository = new TrackingProgressRepository(_context);
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
                UserName = "testuser1@example.com"
            };

            var user2 = new ApplicationUser
            {
                Id = "user2",
                UserName = "testuser2@example.com"
            };

            var course = new Course
            {
                Id = 1,
                Name = "Test Course"
            };

            var courseProgress1 = new CourseProgress
            {
                UserId = "user1",
                CourseId = 1,
            };

            var courseProgress2 = new CourseProgress
            {
                UserId = "user2",
                CourseId = 1,
 
            };

            var step1 = new Step
            {
                Id = 1,
                CourseId = 1,
                Name = "Step 1",
                Course = course
            };

            var step2 = new Step
            {
                Id = 2,
                CourseId = 1,
                Name = "Step 2",
                Course = course
            };

            var trackingProgresses = new[]
            {
                new TrackingProgress
                {
                    Id = 1,
                    UserId = "user1",
                    ProgressId = 1,
                    StepId = 1,
                    User = user1,
                    CourseProgress = courseProgress1,
                    Step = step1,
                    Date = DateTime.UtcNow.AddDays(-2)
                },
                new TrackingProgress
                {
                    Id = 2,
                    UserId = "user1",
                    ProgressId = 1,
                    StepId = 2,
                    User = user1,
                    CourseProgress = courseProgress1,
                    Step = step2,
                    Date = DateTime.UtcNow.AddDays(-1)
                },
                new TrackingProgress
                {
                    Id = 3,
                    UserId = "user2",
                    ProgressId = 2,
                    StepId = 1,
                    User = user2,
                    CourseProgress = courseProgress2,
                    Step = step1,
                    Date = DateTime.UtcNow
                }
            };

            _context.Users.AddRange(user1, user2);
            _context.Courses.Add(course);
            _context.CourseProgresses.AddRange(courseProgress1, courseProgress2);
            _context.Steps.AddRange(step1, step2);
            _context.TrackingProgresses.AddRange(trackingProgresses);
            _context.SaveChanges();
        }

        [Test]
        public async Task GetCompletedStepsCountByUserId_ExistingUser_ReturnsCorrectCount()
        {
            // Act
            var result = await _repository.GetCompletedStepsCountByUserId("user1", 1);

            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public async Task GetCompletedStepsCountByUserId_UserWithSingleStep_ReturnsOne()
        {
            // Act
            var result = await _repository.GetCompletedStepsCountByUserId("user2", 1);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task GetCompletedStepsCountByUserId_NonExistingUser_ReturnsZero()
        {
            // Act
            var result = await _repository.GetCompletedStepsCountByUserId("nonexistentuser", 1);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task GetCompletedStepsCountByUserId_EmptyDatabase_ReturnsZero()
        {
            // Arrange
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();

            // Act
            var result = await _repository.GetCompletedStepsCountByUserId("user1",  1);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task GetCompletedStepsCountByUserId_MultipleStepsAdded_ReturnsUpdatedCount()
        {
            // Arrange
            var newStep = new Step
            {
                Id = 3,
                CourseId = 1,
                Name = "Step 3",
                Course = await _context.Courses.FindAsync(1)
            };

            var newTracking = new TrackingProgress
            {
                Id = 4,
                UserId = "user1",
                ProgressId = 1,
                StepId = 3,
                User = await _context.Users.FindAsync("user1"),
                CourseProgress = await _context.CourseProgresses.FindAsync(1),
                Step = newStep,
                Date = DateTime.UtcNow
            };

            await _context.Steps.AddAsync(newStep);
            await _context.TrackingProgresses.AddAsync(newTracking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCompletedStepsCountByUserId("user1", 1);

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }
    }
}