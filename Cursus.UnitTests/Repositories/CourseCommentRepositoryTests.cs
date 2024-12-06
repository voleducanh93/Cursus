using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Cursus.Data.Models;
using Cursus.Data.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Cursus.Repository.Repository;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class CourseCommentRepositoryTests
    {
        private CursusDbContext _context;
        private CourseCommentRepository _repository;
        private Course _testCourse;
        private ApplicationUser _testUser;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: $"CursusDb_{System.Guid.NewGuid()}")
                .Options;

            _context = new CursusDbContext(options);
            _repository = new CourseCommentRepository(_context);

            // Create test course
            _testCourse = new Course { Id = 1, Name = "Test Course" };
            _context.Courses.Add(_testCourse);

            // Create test user
            _testUser = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            _context.Users.Add(_testUser);

            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddAsync_ShouldAddCourseCommentAndReturnIt()
        {
            // Arrange
            var comment = new CourseComment
            {
                Comment = "Great course!",
                Rating = 4.5,
                CourseId = _testCourse.Id,
                UserId = _testUser.Id,
                DateCreated = DateTime.Now,
                IsFlagged = false
            };

            // Act
            var result = await _repository.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(await _context.CourseComments.CountAsync(), Is.EqualTo(1));
            Assert.That(result.Comment, Is.EqualTo("Great course!"));
            Assert.That(result.Rating, Is.EqualTo(4.5));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveCourseCommentAndReturnIt()
        {
            // Arrange
            var comment = new CourseComment
            {
                Comment = "Great course!",
                Rating = 4.5,
                CourseId = _testCourse.Id,
                UserId = _testUser.Id,
                DateCreated = DateTime.Now
            };
            await _context.CourseComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(comment);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(await _context.CourseComments.CountAsync(), Is.EqualTo(0));
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateCourseCommentAndReturnIt()
        {
            // Arrange
            var comment = new CourseComment
            {
                Comment = "Great course!",
                Rating = 4.5,
                CourseId = _testCourse.Id,
                UserId = _testUser.Id,
                DateCreated = DateTime.Now
            };
            await _context.CourseComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Act
            comment.Comment = "Updated comment";
            comment.Rating = 5.0;
            var result = await _repository.UpdateAsync(comment);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Comment, Is.EqualTo("Updated comment"));
            Assert.That(result.Rating, Is.EqualTo(5.0));
            var updatedComment = await _context.CourseComments.FirstAsync();
            Assert.That(updatedComment.Comment, Is.EqualTo("Updated comment"));
        }

        [Test]
        public async Task GetAsync_WithValidFilter_ShouldReturnCourseComment()
        {
            // Arrange
            var comment = new CourseComment
            {
                Comment = "Great course!",
                Rating = 4.5,
                CourseId = _testCourse.Id,
                UserId = _testUser.Id,
                DateCreated = DateTime.Now
            };
            await _context.CourseComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAsync(c => c.CourseId == _testCourse.Id && c.UserId == _testUser.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.CourseId, Is.EqualTo(_testCourse.Id));
            Assert.That(result.UserId, Is.EqualTo(_testUser.Id));
        }

        [Test]
        public async Task GetAsync_WithInvalidFilter_ShouldReturnNull()
        {
            // Arrange
            var comment = new CourseComment
            {
                Comment = "Great course!",
                Rating = 4.5,
                CourseId = _testCourse.Id,
                UserId = _testUser.Id,
                DateCreated = DateTime.Now
            };
            await _context.CourseComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAsync(c => c.CourseId == 999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_WithNoFilter_ShouldReturnAllCourseComments()
        {
            // Arrange
            var comments = new List<CourseComment>
            {
                new CourseComment { Comment = "Comment 1", Rating = 4.5, CourseId = _testCourse.Id, UserId = _testUser.Id },
                new CourseComment { Comment = "Comment 2", Rating = 3.5, CourseId = _testCourse.Id, UserId = _testUser.Id },
                new CourseComment { Comment = "Comment 3", Rating = 5.0, CourseId = _testCourse.Id, UserId = _testUser.Id }
            };
            await _context.CourseComments.AddRangeAsync(comments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task GetAllAsync_WithFilter_ShouldReturnFilteredCourseComments()
        {
            // Arrange
            var comments = new List<CourseComment>
            {
                new CourseComment { Comment = "Comment 1", Rating = 4.5, CourseId = _testCourse.Id, UserId = _testUser.Id, IsFlagged = true },
                new CourseComment { Comment = "Comment 2", Rating = 3.5, CourseId = _testCourse.Id, UserId = _testUser.Id, IsFlagged = false },
                new CourseComment { Comment = "Comment 3", Rating = 5.0, CourseId = _testCourse.Id, UserId = _testUser.Id, IsFlagged = true }
            };
            await _context.CourseComments.AddRangeAsync(comments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync(c => c.IsFlagged);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetAsync_WithIncludeProperties_ShouldReturnCourseCommentWithRelatedData()
        {
            // Arrange
            var comment = new CourseComment
            {
                Comment = "Great course!",
                Rating = 4.5,
                CourseId = _testCourse.Id,
                UserId = _testUser.Id,
                DateCreated = DateTime.Now
            };
            await _context.CourseComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAsync(
                c => c.CourseId == _testCourse.Id,
                includeProperties: "Course,User");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Course, Is.Not.Null);
            Assert.That(result.User, Is.Not.Null);
            Assert.That(result.Course.Id, Is.EqualTo(_testCourse.Id));
            Assert.That(result.User.Id, Is.EqualTo(_testUser.Id));
        }

        [Test]
        public async Task GetAllAsync_WithIncludeProperties_ShouldReturnCourseCommentsWithRelatedData()
        {
            // Arrange
            var comments = new List<CourseComment>
            {
                new CourseComment { Comment = "Comment 1", Rating = 4.5, CourseId = _testCourse.Id, UserId = _testUser.Id },
                new CourseComment { Comment = "Comment 2", Rating = 3.5, CourseId = _testCourse.Id, UserId = _testUser.Id }
            };
            await _context.CourseComments.AddRangeAsync(comments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync(
                includeProperties: "Course,User");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(c => c.Course != null), Is.True);
            Assert.That(result.All(c => c.User != null), Is.True);
        }

        [Test]
        public async Task GetAllAsync_WithFilterAndInclude_ShouldReturnFilteredCourseCommentsWithRelatedData()
        {
            // Arrange
            var comments = new List<CourseComment>
            {
                new CourseComment { Comment = "Comment 1", Rating = 4.5, CourseId = _testCourse.Id, UserId = _testUser.Id, IsFlagged = true },
                new CourseComment { Comment = "Comment 2", Rating = 3.5, CourseId = _testCourse.Id, UserId = _testUser.Id, IsFlagged = false }
            };
            await _context.CourseComments.AddRangeAsync(comments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync(
                c => c.IsFlagged,
                includeProperties: "Course,User");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Course, Is.Not.Null);
            Assert.That(result.First().User, Is.Not.Null);
            Assert.That(result.First().IsFlagged, Is.True);
        }
    }
}