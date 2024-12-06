using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Cursus.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cursus.Data.Models;
using Cursus.Data.Enum;
using Cursus.Repository.Repository;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class InstructorRepositoryTests
    {
        private CursusDbContext _context;
        private InstructorRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{TestContext.CurrentContext.Test.Name}")
                .Options;

            _context = new CursusDbContext(options);
            _repository = new InstructorRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddAsync_AddsInstructorSuccessfully()
        {
            // Arrange
            var instructor = new InstructorInfo
            {
                UserId = "user1",
                CardName = "Test Card",
                StatusInsructor = InstructorStatus.Approved
            };

            // Act
            await _repository.AddAsync(instructor);

            // Assert
            var result = await _context.InstructorInfos.FirstOrDefaultAsync();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.CardName, Is.EqualTo("Test Card"));
        }

        [Test]
        public async Task DeleteAsync_DeletesInstructorSuccessfully()
        {
            // Arrange
            var instructor = new InstructorInfo
            {
                Id = 1,
                UserId = "user1",
                CardName = "Test Card"
            };
            await _context.InstructorInfos.AddAsync(instructor);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(1);

            // Assert
            var result = await _context.InstructorInfos.FindAsync(1);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByIDAsync_ExistingId_ReturnsInstructor()
        {
            // Arrange
            var instructor = new InstructorInfo
            {
                Id = 1,
                UserId = "user1",
                User = new ApplicationUser { Id = "user1", UserName = "testuser" }
            };
            await _context.InstructorInfos.AddAsync(instructor);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIDAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.User, Is.Not.Null);
            Assert.That(result.User.UserName, Is.EqualTo("testuser"));
        }

        [Test]
        public async Task GetByIDAsync_NonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _repository.GetByIDAsync(999));
        }

        [Test]
        public async Task UpdateAsync_UpdatesInstructorSuccessfully()
        {
            // Arrange
            var instructor = new InstructorInfo
            {
                Id = 1,
                CardName = "Old Card"
            };
            await _context.InstructorInfos.AddAsync(instructor);
            await _context.SaveChangesAsync();

            instructor.CardName = "New Card";

            // Act
            await _repository.UpdateAsync(instructor);

            // Assert
            var updated = await _context.InstructorInfos.FindAsync(1);
            Assert.That(updated.CardName, Is.EqualTo("New Card"));
        }

        [Test]
        public async Task GetAllInstructorsAsync_ReturnsAllInstructors()
        {
            // Arrange
            var instructors = new List<InstructorInfo>
            {
                new InstructorInfo { Id = 1, UserId = "user1", User = new ApplicationUser { Id = "user1" } },
                new InstructorInfo { Id = 2, UserId = "user2", User = new ApplicationUser { Id = "user2" } }
            };
            await _context.InstructorInfos.AddRangeAsync(instructors);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllInstructorsAsync();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task TotalCourse_ReturnsCorrectCount()
        {
            // Arrange
            var instructor = new InstructorInfo { Id = 1 };
            var courses = new List<Course>
            {
                new Course { Id = 1, InstructorInfoId = 1 },
                new Course { Id = 2, InstructorInfoId = 1 },
                new Course { Id = 3, InstructorInfoId = 2 }
            };

            await _context.InstructorInfos.AddAsync(instructor);
            await _context.Courses.AddRangeAsync(courses);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.TotalCourse(1);

            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public async Task TotalActiveCourse_ReturnsCorrectCount()
        {
            // Arrange
            var instructor = new InstructorInfo { Id = 1 };
            var courses = new List<Course>
            {
                new Course { Id = 1, InstructorInfoId = 1, Status = true },
                new Course { Id = 2, InstructorInfoId = 1, Status = true },
                new Course { Id = 3, InstructorInfoId = 1, Status = false }
            };

            await _context.InstructorInfos.AddAsync(instructor);
            await _context.Courses.AddRangeAsync(courses);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.TotalActiveCourse(1);

            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public async Task TotalPayout_ReturnsCorrectAmount()
        {
            // Arrange
            var instructor = new InstructorInfo
            {
                Id = 1,
                TotalEarning = 1000
            };
            await _context.InstructorInfos.AddAsync(instructor);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.TotalPayout(1);

            // Assert
            Assert.That(result, Is.EqualTo(700)); // 70% of 1000
        }

        [Test]
        public async Task RatingNumber_ReturnsCorrectAverageRating()
        {
            // Arrange
            var instructor = new InstructorInfo { Id = 1 };
            var courses = new List<Course>
            {
                new Course { Id = 1, InstructorInfoId = 1, Status = true, Rating = 4.5 },
                new Course { Id = 2, InstructorInfoId = 1, Status = true, Rating = 3.5 },
                new Course { Id = 3, InstructorInfoId = 1, Status = false, Rating = 1.0 }
            };

            await _context.InstructorInfos.AddAsync(instructor);
            await _context.Courses.AddRangeAsync(courses);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.RatingNumber(1);

            // Assert
            Assert.That(result, Is.EqualTo(4.0));
        }

        [Test]
        public async Task RatingNumber_NoActiveCourses_ReturnsZero()
        {
            // Arrange
            var instructor = new InstructorInfo { Id = 1 };
            await _context.InstructorInfos.AddAsync(instructor);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.RatingNumber(1);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllInstructors_IncludesUserData()
        {
            // Arrange
            var instructors = new List<InstructorInfo>
            {
                new InstructorInfo
                {
                    Id = 1,
                    UserId = "user1",
                    User = new ApplicationUser { Id = "user1", UserName = "test1" }
                },
                new InstructorInfo
                {
                    Id = 2,
                    UserId = "user2",
                    User = new ApplicationUser { Id = "user2", UserName = "test2" }
                }
            };
            await _context.InstructorInfos.AddRangeAsync(instructors);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllInstructors();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(i => i.User != null), Is.True);
        }
    }
}