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
    public class ReasonRepositoryTests
    {
        private CursusDbContext _context;
        private ReasonRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: $"CursusDb_{Guid.NewGuid()}")
                .Options;

            _context = new CursusDbContext(options);
            _repository = new ReasonRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddAsync_ShouldAddReasonToDatabase()
        {
            // Arrange
            var reason = new Reason
            {
                Description = "Test Reason",
                CourseId = 1,
                Status = 1
            };

            // Act
            var result = await _repository.AddAsync(reason);
            await _context.SaveChangesAsync();

            // Assert
            var savedReason = await _context.Set<Reason>().FirstOrDefaultAsync();
            Assert.That(savedReason, Is.Not.Null);
            Assert.That(savedReason.Description, Is.EqualTo(reason.Description));
            Assert.That(savedReason.CourseId, Is.EqualTo(reason.CourseId));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveReasonFromDatabase()
        {
            // Arrange
            var reason = new Reason
            {
                Description = "Test Reason",
                CourseId = 1,
                Status = 1
            };
            await _context.AddAsync(reason);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(reason);
            await _context.SaveChangesAsync();

            // Assert
            var deletedReason = await _context.Set<Reason>().FirstOrDefaultAsync();
            Assert.That(deletedReason, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllReasons()
        {
            // Arrange
            var reasonList = new List<Reason>
            {
                new Reason { Description = "Reason 1", CourseId = 1, Status = 1 },
                new Reason { Description = "Reason 2", CourseId = 2, Status = 1 }
            };
            await _context.AddRangeAsync(reasonList);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            CollectionAssert.AreEquivalent(reasonList, result);
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnReason()
        {
            // Arrange
            var reason = new Reason
            {
                Id = 1,
                Description = "Test Reason",
                CourseId = 1,
                Status = 1
            };
            await _context.AddAsync(reason);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Description, Is.EqualTo("Test Reason"));
        }

        [Test]
        public void GetByIdAsync_ShouldThrowKeyNotFoundException_WhenReasonNotFound()
        {
            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _repository.GetByIdAsync(999));
        }

        [Test]
        public async Task GetByCourseIdAsync_ShouldReturnReason()
        {
            // Arrange
            var reason = new Reason
            {
                Description = "Test Reason",
                CourseId = 1,
                Status = 1
            };
            await _context.AddAsync(reason);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByCourseIdAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.CourseId, Is.EqualTo(1));
            Assert.That(result.Description, Is.EqualTo("Test Reason"));
        }

        [Test]
        public void GetByCourseIdAsync_ShouldThrowKeyNotFoundException_WhenReasonNotFound()
        {
            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _repository.GetByCourseIdAsync(999));
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateReasonInDatabase()
        {
            // Arrange
            var reason = new Reason
            {
                Description = "Test Reason",
                CourseId = 1,
                Status = 1
            };
            await _context.AddAsync(reason);
            await _context.SaveChangesAsync();

            // Act
            reason.Description = "Updated Reason";
            await _repository.UpdateAsync(reason);
            await _context.SaveChangesAsync();

            // Assert
            var updatedReason = await _context.Set<Reason>().FirstOrDefaultAsync();
            Assert.That(updatedReason, Is.Not.Null);
            Assert.That(updatedReason.Description, Is.EqualTo("Updated Reason"));
        }

        [Test]
        public async Task GetAllAsync_WithFilter_ShouldReturnFilteredReasons()
        {
            // Arrange
            var reasonList = new List<Reason>
            {
                new Reason { Description = "Reason 1", CourseId = 1, Status = 1 },
                new Reason { Description = "Reason 2", CourseId = 2, Status = 2 }
            };
            await _context.AddRangeAsync(reasonList);
            await _context.SaveChangesAsync();

            Expression<Func<Reason, bool>> filter = x => x.Status == 1;

            // Act
            var result = await _repository.GetAllAsync(filter);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Status, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAllAsync_WithIncludeProperties_ShouldIncludeRelatedEntities()
        {
            // Arrange
            var course = new Course { Id = 1, Name = "Test Course" };
            var reason = new Reason
            {
                Description = "Test Reason",
                CourseId = 1,
                Status = 1,
                Course = course
            };
            await _context.AddAsync(course);
            await _context.AddAsync(reason);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync(includeProperties: "Course");

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Course, Is.Not.Null);
            Assert.That(result.First().Course.Name, Is.EqualTo("Test Course"));
        }
    }
}