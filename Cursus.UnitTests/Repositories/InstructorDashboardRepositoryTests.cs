using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Moq;
using Cursus.Data.DTO;
using Cursus.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cursus.Data.Entities;
using Cursus.Repository.Repository;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class InstructorDashboardRepositoryTests
    {
        private CursusDbContext _context;
        private InstructorDashboardRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{TestContext.CurrentContext.Test.Name}")
                .Options;

            _context = new CursusDbContext(options);
            _repository = new InstructorDashboardRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetInstructorDashboardAsync_ReturnsCorrectData()
        {
            // Arrange
            var instructorId = 1;
            var instructor = new InstructorInfo
            {
                Id = instructorId,
                TotalEarning = 1000
            };

            var courses = new List<Course>
            {
                new Course { Id = 1, InstructorInfo = instructor, Price = 100, Status = true },
                new Course { Id = 2, InstructorInfo = instructor, Price = 200, Status = true },
                new Course { Id = 3, InstructorInfo = instructor, Price = 300, Status = false }
            };

            await _context.InstructorInfos.AddAsync(instructor);
            await _context.Courses.AddRangeAsync(courses);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetInstructorDashboardAsync(instructorId);

            // Assert
            Assert.That(result.TotalPotentialEarnings, Is.EqualTo(300)); // Sum of active courses only
            Assert.That(result.TotalCourses, Is.EqualTo(2)); // Count of active courses
            Assert.That(result.TotalEarnings, Is.EqualTo(1000));
        }

        [Test]
        public async Task GetInstructorDashboardAsync_NoInstructor_ReturnsEmptyDashboard()
        {
            // Act
            var result = await _repository.GetInstructorDashboardAsync(999);

            // Assert
            Assert.That(result.TotalPotentialEarnings, Is.EqualTo(0));
            Assert.That(result.TotalCourses, Is.EqualTo(0));
            Assert.That(result.TotalEarnings, Is.EqualTo(0));
        }

        [Test]
        public async Task GetCourseEarningsAsync_ReturnsCorrectEarnings()
        {
            // Arrange
            var instructorId = 1;
            var instructor = new InstructorInfo { Id = instructorId };

            var courses = new List<Course>
            {
                new Course {
                    Id = 1,
                    InstructorInfo = instructor,
                    Price = 100,
                    Status = true,
                    Description = "Test Course 1"
                },
                new Course {
                    Id = 2,
                    InstructorInfo = instructor,
                    Price = 200,
                    Status = true,
                    Description = "Test Course 2"
                }
            };

            var courseProgresses = new List<CourseProgress>
            {
                new CourseProgress { CourseId = 1, UserId = "1" },
                new CourseProgress { CourseId = 1, UserId = "2" },
                new CourseProgress { CourseId = 2, UserId = "1" }
            };

            await _context.InstructorInfos.AddAsync(instructor);
            await _context.Courses.AddRangeAsync(courses);
            await _context.CourseProgresses.AddRangeAsync(courseProgresses);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCourseEarningsAsync(instructorId);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Earnings, Is.EqualTo(200)); // 2 enrollments * 100
            Assert.That(result[1].Earnings, Is.EqualTo(200)); // 1 enrollment * 200
        }

        [Test]
        public async Task GetCourseEarningsAsync_NoEnrollments_ReturnsZeroEarnings()
        {
            // Arrange
            var instructorId = 1;
            var instructor = new InstructorInfo { Id = instructorId };

            var course = new Course
            {
                Id = 1,
                InstructorInfo = instructor,
                Price = 100,
                Status = true,
                Description = "Test Course"
            };

            await _context.InstructorInfos.AddAsync(instructor);
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCourseEarningsAsync(instructorId);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Earnings, Is.EqualTo(0));
        }

        [Test]
        public async Task GetCourseEarningsAsync_LongDescription_TruncatesCorrectly()
        {
            // Arrange
            var instructorId = 1;
            var instructor = new InstructorInfo { Id = instructorId };
            var longDescription = new string('x', 150);

            var course = new Course
            {
                Id = 1,
                InstructorInfo = instructor,
                Price = 100,
                Status = true,
                Description = longDescription
            };

            await _context.InstructorInfos.AddAsync(instructor);
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCourseEarningsAsync(instructorId);

            // Assert
            Assert.That(result[0].ShortSummary.Length, Is.EqualTo(100));
        }

        [Test]
        public async Task GetCourseEarningsAsync_NoCoursesFound_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetCourseEarningsAsync(999);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetCourseEarningsAsync_InactiveCourses_NotIncluded()
        {
            // Arrange
            var instructorId = 1;
            var instructor = new InstructorInfo { Id = instructorId };

            var courses = new List<Course>
            {
                new Course {
                    Id = 1,
                    InstructorInfo = instructor,
                    Price = 100,
                    Status = true,
                    Description = "Active Course"
                },
                new Course {
                    Id = 2,
                    InstructorInfo = instructor,
                    Price = 200,
                    Status = false,
                    Description = "Inactive Course"
                }
            };

            await _context.InstructorInfos.AddAsync(instructor);
            await _context.Courses.AddRangeAsync(courses);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCourseEarningsAsync(instructorId);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Status, Is.EqualTo("Active"));
        }
    }
}