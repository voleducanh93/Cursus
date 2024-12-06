using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.Repository.Repository;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class StepContentRepositoryTests
    {
        private CursusDbContext _context;
        private StepContentRepository _repository;
        private DbContextOptions<CursusDbContext> _options;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: "TestStepContentDb")
                .Options;
        }

        [SetUp]
        public void Setup()
        {
            _context = new CursusDbContext(_options);
            _repository = new StepContentRepository(_context);
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
            var step = new Step
            {
                Id = 1,
                Name = "Test Step"
            };

            var stepContents = new[]
            {
                new StepContent
                {
                    Id = 1,
                    StepId = 1,
                    ContentType = "video",
                    ContentURL = "https://example.com/video1",
                    DateCreated = DateTime.UtcNow,
                    Description = "Test Video Content",
                    Step = step
                },
                new StepContent
                {
                    Id = 2,
                    StepId = 1,
                    ContentType = "document",
                    ContentURL = "https://example.com/doc1",
                    DateCreated = DateTime.UtcNow,
                    Description = "Test Document Content",
                    Step = step
                }
            };

            _context.Steps.Add(step);
            _context.StepContents.AddRange(stepContents);
            _context.SaveChanges();
        }

        [Test]
        public async Task GetByIdAsync_ExistingId_ReturnsStepContent()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.ContentType, Is.EqualTo("video"));
        }

        [Test]
        public void GetByIdAsync_NonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _repository.GetByIdAsync(999));
        }

        [Test]
        public async Task FirstOrDefaultAsync_ExistingCondition_ReturnsStepContent()
        {
            // Arrange
            Expression<Func<StepContent, bool>> predicate = sc => sc.ContentType == "video";

            // Act
            var result = await _repository.FirstOrDefaultAsync(predicate);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ContentType, Is.EqualTo("video"));
        }

        [Test]
        public async Task FirstOrDefaultAsync_NonExistingCondition_ReturnsNull()
        {
            // Arrange
            Expression<Func<StepContent, bool>> predicate = sc => sc.ContentType == "nonexistent";

            // Act
            var result = await _repository.FirstOrDefaultAsync(predicate);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task FirstOrDefaultAsync_MultipleMatches_ReturnsFirstMatch()
        {
            // Arrange
            var newContent = new StepContent
            {
                Id = 3,
                StepId = 1,
                ContentType = "video",
                ContentURL = "https://example.com/video2",
                DateCreated = DateTime.UtcNow,
                Description = "Another Test Video Content",
                Step = await _context.Steps.FindAsync(1)
            };
            _context.StepContents.Add(newContent);
            await _context.SaveChangesAsync();

            Expression<Func<StepContent, bool>> predicate = sc => sc.ContentType == "video";

            // Act
            var result = await _repository.FirstOrDefaultAsync(predicate);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task FirstOrDefaultAsync_ComplexPredicate_ReturnsCorrectResult()
        {
            // Arrange
            Expression<Func<StepContent, bool>> predicate =
                sc => sc.ContentType == "video" && sc.ContentURL.Contains("example.com");

            // Act
            var result = await _repository.FirstOrDefaultAsync(predicate);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ContentType, Is.EqualTo("video"));
            Assert.That(result.ContentURL, Does.Contain("example.com"));
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