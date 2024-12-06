using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Cursus.Data.Models;
using Cursus.Data.Entities;
using System.Threading.Tasks;
using Cursus.Repository.Repository;
using Cursus.Data.Enums;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class CourseRepositoryTests
    {
        private CursusDbContext _context;
        private CourseRepository _repository;
        private DbContextOptions<CursusDbContext> _options;

        [OneTimeSetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: "TestCursusDb_Course")
                .Options;
        }

        [SetUp]
        public void SetupTest()
        {
            _context = new CursusDbContext(_options);
            _repository = new CourseRepository(_context);
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
            var category = new Category { Id = 1, Name = "Test Category" };
            var instructor = new InstructorInfo { Id = 1};

            _context.Categories.Add(category);
            _context.InstructorInfos.Add(instructor);

            var courses = new[]
            {
                new Course
                {
                    Id = 1,
                    Name = "Course 1",
                    CategoryId = 1,
                    InstructorInfoId = 1,
                    Status = true,
                    IsApprove = ApproveStatus.Pending,
                    Rating = 4.5,
                    Price = 99.99,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now
                },
                new Course
                {
                    Id = 2,
                    Name = "Course 2",
                    CategoryId = 1,
                    InstructorInfoId = 1,
                    Status = true,
                    IsApprove = ApproveStatus.Approved,
                    Rating = 0,
                    Price = 149.99,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now
                }
            };

            var comments = new[]
            {
                new CourseComment { Id = 1, CourseId = 1, Rating = 4.0, IsFlagged = false },
                new CourseComment { Id = 2, CourseId = 1, Rating = 5.0, IsFlagged = false },
                new CourseComment { Id = 3, CourseId = 1, Rating = 3.0, IsFlagged = true }
            };

            _context.Courses.AddRange(courses);
            _context.CourseComments.AddRange(comments);
            _context.SaveChanges();
        }

        [Test]
        public async Task AnyAsync_WithExistingCondition_ReturnsTrue()
        {
            var result = await _repository.AnyAsync(c => c.Name == "Course 1");
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task AnyAsync_WithNonExistingCondition_ReturnsFalse()
        {
            var result = await _repository.AnyAsync(c => c.Name == "Non Existent Course");
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task GetAllIncludeStepsAsync_WithValidId_ReturnsCourse()
        {
            var result = await _repository.GetAllIncludeStepsAsync(1);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAllIncludeStepsAsync_WithInvalidId_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _repository.GetAllIncludeStepsAsync(999));
        }

        [Test]
        public async Task UpdateCourseRating_WithValidId_UpdatesRating()
        {
            await _repository.UpdateCourseRating(1);
            var course = await _context.Courses.FindAsync(1);
            Assert.That(course.Rating, Is.EqualTo(4.5));
        }

        [Test]
        public async Task UpdateCourseRating_WithNoValidComments_SetsRatingToZero()
        {
            await _repository.UpdateCourseRating(2);
            var course = await _context.Courses.FindAsync(2);
            Assert.That(course.Rating, Is.EqualTo(0));
        }

        [Test]
        public async Task UpdateCourseRating_WithInvalidId_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _repository.UpdateCourseRating(999));
        }

        [Test]
        public async Task ApproveCourse_WithApproveChoice_UpdatesStatusToApproved()
        {
            await _repository.ApproveCourse(1, true);
            var course = await _context.Courses.FindAsync(1);
            Assert.That(course.IsApprove, Is.EqualTo(ApproveStatus.Approved));
            Assert.That(course.Status, Is.True);
        }

        [Test]
        public async Task ApproveCourse_WithDenyChoice_UpdatesStatusToDeniedAndInactive()
        {
            await _repository.ApproveCourse(1, false);
            var course = await _context.Courses.FindAsync(1);
            Assert.That(course.IsApprove, Is.EqualTo(ApproveStatus.Denied));
            Assert.That(course.Status, Is.False);
        }

        [Test]
        public async Task CountAsync_WithValidPredicate_ReturnsCorrectCount()
        {
            var result = await _repository.CountAsync(c => c.Status == true);
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public async Task CountAsync_WithNonMatchingPredicate_ReturnsZero()
        {
            var result = await _repository.CountAsync(c => c.Price > 1000);
            Assert.That(result, Is.Zero);
        }

        [Test]
        public async Task AddAsync_CreatesCourse()
        {
            var newCourse = new Course
            {
                Name = "New Course",
                CategoryId = 1,
                InstructorInfoId = 1,
                Status = true,
                Price = 199.99,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now
            };

            await _repository.AddAsync(newCourse);
            await _context.SaveChangesAsync();

            var result = await _context.Courses.FindAsync(newCourse.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("New Course"));
        }

        [Test]
        public async Task DeleteAsync_RemovesCourse()
        {
            var course = await _context.Courses.FindAsync(1);
            await _repository.DeleteAsync(course);
            await _context.SaveChangesAsync();

            var result = await _context.Courses.FindAsync(1);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task UpdateAsync_ModifiesCourse()
        {
            var course = await _context.Courses.FindAsync(1);
            course.Name = "Updated Course Name";

            await _repository.UpdateAsync(course);
            await _context.SaveChangesAsync();

            var result = await _context.Courses.FindAsync(1);
            Assert.That(result.Name, Is.EqualTo("Updated Course Name"));
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllCourses()
        {
            var results = await _repository.GetAllAsync();
            Assert.That(results.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllAsync_WithFilter_ReturnsFilteredCourses()
        {
            var results = await _repository.GetAllAsync(c => c.Price > 100);
            Assert.That(results.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetAllAsync_WithInclude_ReturnsRelatedData()
        {
            var results = await _repository.GetAllAsync(includeProperties: "Category,InstructorInfo");
            Assert.That(results.All(c => c.Category != null && c.InstructorInfo != null), Is.True);
        }
    }
}