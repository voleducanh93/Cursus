using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Linq.Expressions;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class ProgressRepositoryTests
    {
        private CursusDbContext _context;
        private ProgressRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: $"CursusDb_{Guid.NewGuid()}")
                .Options;

            _context = new CursusDbContext(options);
            _repository = new ProgressRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddAsync_ShouldAddProgressToDatabase()
        {
            // Arrange
            var progress = new CourseProgress
            {
                CourseId = 1,
                UserId = "user1",
                Type = "Video",
                Date = DateTime.UtcNow,
                IsCompleted = false
            };

            // Act
            var result = await _repository.AddAsync(progress);
            await _context.SaveChangesAsync();

            // Assert
            var savedProgress = await _context.Set<CourseProgress>().FirstOrDefaultAsync();
            Assert.That(savedProgress, Is.Not.Null);
            Assert.That(savedProgress.CourseId, Is.EqualTo(progress.CourseId));
            Assert.That(savedProgress.UserId, Is.EqualTo(progress.UserId));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveProgressFromDatabase()
        {
            // Arrange
            var progress = new CourseProgress
            {
                CourseId = 1,
                UserId = "user1",
                Type = "Video",
                Date = DateTime.UtcNow,
                IsCompleted = false
            };
            await _context.AddAsync(progress);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(progress);
            await _context.SaveChangesAsync();

            // Assert
            var deletedProgress = await _context.Set<CourseProgress>().FirstOrDefaultAsync();
            Assert.That(deletedProgress, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllProgress()
        {
            // Arrange
            var progressList = new List<CourseProgress>
            {
                new CourseProgress { CourseId = 1, UserId = "user1", Type = "Video", Date = DateTime.UtcNow, IsCompleted = false },
                new CourseProgress { CourseId = 2, UserId = "user2", Type = "Quiz", Date = DateTime.UtcNow, IsCompleted = true }
            };
            await _context.AddRangeAsync(progressList);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            CollectionAssert.AreEquivalent(progressList, result);
        }

        [Test]
        public async Task GetAllAsync_WithFilter_ShouldReturnFilteredProgress()
        {
            // Arrange
            var progressList = new List<CourseProgress>
            {
                new CourseProgress { CourseId = 1, UserId = "user1", Type = "Video", Date = DateTime.UtcNow, IsCompleted = false },
                new CourseProgress { CourseId = 2, UserId = "user2", Type = "Quiz", Date = DateTime.UtcNow, IsCompleted = true }
            };
            await _context.AddRangeAsync(progressList);
            await _context.SaveChangesAsync();

            Expression<Func<CourseProgress, bool>> filter = x => x.Type == "Video";

            // Act
            var result = await _repository.GetAllAsync(filter);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Type, Is.EqualTo("Video"));
        }

        [Test]
        public async Task GetAsync_ShouldReturnSpecificProgress()
        {
            // Arrange
            var progress = new CourseProgress
            {
                CourseId = 1,
                UserId = "user1",
                Type = "Video",
                Date = DateTime.UtcNow,
                IsCompleted = false
            };
            await _context.AddAsync(progress);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAsync(x => x.CourseId == 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.CourseId, Is.EqualTo(1));
            Assert.That(result.UserId, Is.EqualTo("user1"));
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateProgressInDatabase()
        {
            // Arrange
            var progress = new CourseProgress
            {
                CourseId = 1,
                UserId = "user1",
                Type = "Video",
                Date = DateTime.UtcNow,
                IsCompleted = false
            };
            await _context.AddAsync(progress);
            await _context.SaveChangesAsync();

            // Act
            progress.IsCompleted = true;
            await _repository.UpdateAsync(progress);
            await _context.SaveChangesAsync();

            // Assert
            var updatedProgress = await _context.Set<CourseProgress>().FirstOrDefaultAsync();
            Assert.That(updatedProgress, Is.Not.Null);
            Assert.That(updatedProgress.IsCompleted, Is.True);
        }

        [Test]
        public async Task GetAllAsync_WithIncludeProperties_ShouldIncludeRelatedEntities()
        {
            // Arrange
            var course = new Course { Id = 1, Name = "Test Course" };
            var progress = new CourseProgress
            {
                CourseId = 1,
                UserId = "user1",
                Type = "Video",
                Date = DateTime.UtcNow,
                IsCompleted = false,
                Course = course
            };
            await _context.AddAsync(course);
            await _context.AddAsync(progress);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync(includeProperties: "Course");

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Course, Is.Not.Null);
            Assert.That(result.First().Course.Name, Is.EqualTo("Test Course"));
        }

        [Test]
        public async Task GetAsync_WithIncludeProperties_ShouldIncludeRelatedEntities()
        {
            // Arrange
            var course = new Course { Id = 1, Name = "Test Course" };
            var progress = new CourseProgress
            {
                CourseId = 1,
                UserId = "user1",
                Type = "Video",
                Date = DateTime.UtcNow,
                IsCompleted = false,
                Course = course
            };
            await _context.AddAsync(course);
            await _context.AddAsync(progress);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAsync(x => x.CourseId == 1, includeProperties: "Course");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Course, Is.Not.Null);
            Assert.That(result.Course.Name, Is.EqualTo("Test Course"));
        }
    }
}