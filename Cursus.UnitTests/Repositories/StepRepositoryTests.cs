using NUnit.Framework;
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
    public class StepRepositoryTests
    {
        private CursusDbContext _context;
        private StepRepository _repository;
        private DbContextOptions<CursusDbContext> _options;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: "TestStepDb")
                .Options;
        }

        [SetUp]
        public void Setup()
        {
            _context = new CursusDbContext(_options);
            _repository = new StepRepository(_context);
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
            var course1 = new Course
            {
                Id = 1,
                Name = "Test Course 1"
            };

            var course2 = new Course
            {
                Id = 2,
                Name = "Test Course 2"
            };

            var steps = new List<Step>
            {
                new Step
                {
                    Id = 1,
                    CourseId = 1,
                    Name = "Step 1",
                    Order = 1,
                    Description = "First Step",
                    DateCreated = DateTime.UtcNow,
                    Course = course1,
                    StepComments = new List<StepComment>()
                },
                new Step
                {
                    Id = 2,
                    CourseId = 1,
                    Name = "Step 2",
                    Order = 2,
                    Description = "Second Step",
                    DateCreated = DateTime.UtcNow,
                    Course = course1,
                    StepComments = new List<StepComment>()
                },
                new Step
                {
                    Id = 3,
                    CourseId = 2,
                    Name = "Step 3",
                    Order = 1,
                    Description = "Third Step",
                    DateCreated = DateTime.UtcNow,
                    Course = course2,
                    StepComments = new List<StepComment>()
                }
            };

            _context.Courses.AddRange(course1, course2);
            _context.Steps.AddRange(steps);
            _context.SaveChanges();
        }

        [Test]
        public async Task GetByIdAsync_ExistingId_ReturnsStep()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("Step 1"));
        }

        [Test]
        public void GetByIdAsync_NonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _repository.GetByIdAsync(999));
        }

        [Test]
        public async Task GetByCoursId_ExistingId_ReturnsStep()
        {
            // Act
            var result = await _repository.GetByCoursId(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.CourseId, Is.EqualTo(1));
        }

        [Test]
        public void GetByCoursId_NonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _repository.GetByCoursId(999));
        }

        [Test]
        public async Task GetStepsByCoursId_ExistingCourseId_ReturnsAllSteps()
        {
            // Act
            var results = await _repository.GetStepsByCoursId(1);

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count(), Is.EqualTo(2));
            Assert.That(results.All(s => s.CourseId == 1), Is.True);
        }

        [Test]
        public async Task GetStepsByCoursId_NonExistingCourseId_ReturnsEmptyList()
        {
            // Act
            var results = await _repository.GetStepsByCoursId(999);

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task GetToTalSteps_ExistingCourseId_ReturnsCorrectCount()
        {
            // Act
            var result = await _repository.GetToTalSteps(1);

            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public async Task GetToTalSteps_NonExistingCourseId_ReturnsZero()
        {
            // Act
            var result = await _repository.GetToTalSteps(999);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task GetToTalSteps_MultipleStepsInCourse_ReturnsCorrectCount()
        {
            // Arrange
            var newStep = new Step
            {
                Id = 4,
                CourseId = 1,
                Name = "Step 4",
                Order = 3,
                Description = "Fourth Step",
                DateCreated = DateTime.UtcNow,
                Course = await _context.Courses.FindAsync(1),
                StepComments = new List<StepComment>()
            };
            _context.Steps.Add(newStep);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetToTalSteps(1);

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public async Task GetByIdAsync_NullResult_ThrowsKeyNotFoundException()
        {
            // Arrange
            await _context.Database.EnsureDeletedAsync();

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _repository.GetByIdAsync(1));
        }
    }
}