using NUnit.Framework;
using Moq;
using Microsoft.EntityFrameworkCore;
using Cursus.Data.Models;
using Cursus.Data.Entities;
using System.Linq;
using System.Threading.Tasks;
using Cursus.Repository.Repository;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class CourseProgressRepositoryTests
    {
        private CursusDbContext _context;
        private CourseProgressRepository _repository;
        private DbContextOptions<CursusDbContext> _options;

        [OneTimeSetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: "TestCursusDb")
                .Options;
        }

        [SetUp]
        public void SetupTest()
        {
            _context = new CursusDbContext(_options);
            _repository = new CourseProgressRepository(_context);
            SeedDatabase();
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();

        }

        private void SeedDatabase()
        {
            var user = new ApplicationUser { Id = "user1" };
            var course = new Course { Id = 1, Name = "Test Course" };

            _context.Users.Add(user);
            _context.Courses.Add(course);

            var progresses = new[]
            {
                new CourseProgress { ProgressId = 1, CourseId = 1, UserId = "user1", IsCompleted = true, Type = "Video", Date = DateTime.Now },
                new CourseProgress { ProgressId = 2, CourseId = 1, UserId = "user1", IsCompleted = false, Type = "Quiz", Date = DateTime.Now },
                new CourseProgress { ProgressId = 3, CourseId = 1, UserId = "user1", IsCompleted = true, Type = "Assignment", Date = DateTime.Now },
            };

            _context.CourseProgresses.AddRange(progresses);
            _context.SaveChanges();
        }

        [Test]
        public async Task TrackingProgress_ReturnsTotalAndCompletedCount()
        {
            // Act
            var result = await _repository.TrackingProgress("user1", 1);

            // Assert
            Assert.That(result.total, Is.EqualTo(3));
            Assert.That(result.completed, Is.EqualTo(2));
        }

        [Test]
        public async Task TrackingProgress_WithNonExistentUser_ReturnsZeroCounts()
        {
            // Act
            var result = await _repository.TrackingProgress("nonexistent", 1);

            // Assert
            Assert.That(result.total, Is.EqualTo(0));
            Assert.That(result.completed, Is.EqualTo(0));
        }

        [Test]
        public async Task TrackingProgress_WithNonExistentCourse_ReturnsZeroCounts()
        {
            // Act
            var result = await _repository.TrackingProgress("user1", 999);

            // Assert
            Assert.That(result.total, Is.EqualTo(0));
            Assert.That(result.completed, Is.EqualTo(0));
        }

        [Test]
        public async Task AddAsync_AddsNewCourseProgress()
        {
            // Arrange
            var newProgress = new CourseProgress
            {
                CourseId = 1,
                UserId = "user1",
                Type = "Video",
                Date = DateTime.Now,
                IsCompleted = false
            };

            // Act
            await _repository.AddAsync(newProgress);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.CourseProgresses.FindAsync(newProgress.ProgressId);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Type, Is.EqualTo("Video"));
        }

        [Test]
        public async Task DeleteAsync_RemovesCourseProgress()
        {
            // Arrange
            var progress = await _context.CourseProgresses.FirstAsync();

            // Act
            await _repository.DeleteAsync(progress);
            await _context.SaveChangesAsync();

            // Assert
            var deleted = await _context.CourseProgresses.FindAsync(progress.ProgressId);
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public async Task UpdateAsync_ModifiesCourseProgress()
        {
            // Arrange
            var progress = await _context.CourseProgresses.FirstAsync();
            progress.IsCompleted = !progress.IsCompleted;

            // Act
            await _repository.UpdateAsync(progress);
            await _context.SaveChangesAsync();

            // Assert
            var updated = await _context.CourseProgresses.FindAsync(progress.ProgressId);
            Assert.That(updated.IsCompleted, Is.EqualTo(progress.IsCompleted));
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllProgressForUser()
        {
            // Act
            var results = await _repository.GetAllAsync(x => x.UserId == "user1");

            // Assert
            Assert.That(results.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task GetAllAsync_WithInclude_ReturnsCourseData()
        {
            // Act
            var results = await _repository.GetAllAsync(x => x.UserId == "user1", includeProperties: "Course");

            // Assert
            Assert.That(results.All(x => x.Course != null), Is.True);
        }

        [Test]
        public async Task GetAsync_ReturnsSpecificProgress()
        {
            // Act
            var result = await _repository.GetAsync(x => x.ProgressId == 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ProgressId, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAsync_WithInclude_ReturnsCourseAndUserData()
        {
            // Act
            var result = await _repository.GetAsync(x => x.ProgressId == 1, includeProperties: "Course,ApplicationUser");

            // Assert
            Assert.That(result.Course, Is.Not.Null);
            Assert.That(result.ApplicationUser, Is.Not.Null);
        }
    }
}