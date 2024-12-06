using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Moq;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.Repository.Repository;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class TermPolicyRepositoryTests
    {
        private CursusDbContext _dbContext;
        private TermPolicyRepository _repository;

        [SetUp]
        public void Setup()
        {
            // Create in-memory database options
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Create a new database context for each test
            _dbContext = new CursusDbContext(options);
            _repository = new TermPolicyRepository(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task AddAsync_NewTerm_ShouldAddTermToDatabase()
        {
            // Arrange
            var term = new Term
            {
                Content = "Test Term",
                LastUpdatedBy = "TestUser",
                LastUpdatedDate = DateTime.UtcNow
            };

            // Act
            var addedTerm = await _repository.AddAsync(term);
            await _dbContext.SaveChangesAsync();

            // Assert
            var retrievedTerm = await _dbContext.Terms.FirstOrDefaultAsync(t => t.Id == addedTerm.Id);
            Assert.That(retrievedTerm,Is.Not.Null);
            Assert.That(retrievedTerm.Content,Is.EqualTo(term.Content));
            Assert.That(retrievedTerm.LastUpdatedBy, Is.EqualTo(term.LastUpdatedBy));
        }

        [Test]
        public async Task UpdateAsync_ExistingTerm_ShouldUpdateTermInDatabase()
        {
            // Arrange
            var term = new Term
            {
                Content = "Original Term",
                LastUpdatedBy = "OriginalUser",
                LastUpdatedDate = DateTime.UtcNow
            };

            _dbContext.Terms.Add(term);
            await _dbContext.SaveChangesAsync();

            // Act
            term.Content = "Updated Term";
            term.LastUpdatedBy = "UpdatedUser";
            await _repository.UpdateAsync(term);
            await _dbContext.SaveChangesAsync();

            // Assert
            var updatedTerm = await _dbContext.Terms.FindAsync(term.Id);
            Assert.That(updatedTerm, Is.Not.Null);
            Assert.That(updatedTerm.Content, Is.EqualTo("Updated Term"));
            Assert.That(updatedTerm.LastUpdatedBy, Is.EqualTo("UpdatedUser"));
        }

        [Test]
        public async Task DeleteAsync_ExistingTerm_ShouldRemoveTermFromDatabase()
        {
            // Arrange
            var term = new Term
            {
                Content = "Term to Delete",
                LastUpdatedBy = "DeleteUser",
                LastUpdatedDate = DateTime.UtcNow
            };

            _dbContext.Terms.Add(term);
            await _dbContext.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(term);
            await _dbContext.SaveChangesAsync();

            // Assert
            var deletedTerm = await _dbContext.Terms.FindAsync(term.Id);
            Assert.That(deletedTerm, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_WithNoFilter_ShouldReturnAllTerms()
        {
            // Arrange
            var terms = new[]
            {
                new Term { Content = "Term 1", LastUpdatedBy = "User1", LastUpdatedDate = DateTime.UtcNow },
                new Term { Content = "Term 2", LastUpdatedBy = "User2", LastUpdatedDate = DateTime.UtcNow }
            };

            _dbContext.Terms.AddRange(terms);
            await _dbContext.SaveChangesAsync();

            // Act
            var retrievedTerms = await _repository.GetAllAsync();

            // Assert
            Assert.That(retrievedTerms.Count, Is.EqualTo(2));
            Assert.That(retrievedTerms.Any(t => t.Content == "Term 1"),Is.True);
            Assert.That(retrievedTerms.Any(t => t.Content == "Term 2"),Is.True);
        }

        [Test]
        public async Task GetAllAsync_WithFilter_ShouldReturnFilteredTerms()
        {
            // Arrange
            var terms = new[]
            {
                new Term { Content = "Term 1", LastUpdatedBy = "User1", LastUpdatedDate = DateTime.UtcNow },
                new Term { Content = "Term 2", LastUpdatedBy = "User2", LastUpdatedDate = DateTime.UtcNow },
                new Term { Content = "Term 3", LastUpdatedBy = "User1", LastUpdatedDate = DateTime.UtcNow }
            };

            _dbContext.Terms.AddRange(terms);
            await _dbContext.SaveChangesAsync();

            // Act
            var retrievedTerms = await _repository.GetAllAsync(t => t.LastUpdatedBy == "User1");

            // Assert
            Assert.That(retrievedTerms.Count, Is.EqualTo(2));
            Assert.That(retrievedTerms.All(t => t.LastUpdatedBy == "User1"),Is.True);
        }

        [Test]
        public async Task GetAsync_WithFilter_ShouldReturnFirstMatchingTerm()
        {
            // Arrange
            var terms = new[]
            {
                new Term { Content = "Term 1", LastUpdatedBy = "User1", LastUpdatedDate = DateTime.UtcNow },
                new Term { Content = "Term 2", LastUpdatedBy = "User2", LastUpdatedDate = DateTime.UtcNow }
            };

            _dbContext.Terms.AddRange(terms);
            await _dbContext.SaveChangesAsync();

            // Act
            var retrievedTerm = await _repository.GetAsync(t => t.LastUpdatedBy == "User2");

            // Assert
            Assert.That(retrievedTerm, Is.Not.Null);
            Assert.That(retrievedTerm.LastUpdatedBy, Is.EqualTo("User2"));
            Assert.That(retrievedTerm.Content, Is.EqualTo("Term 2"));
        }

        [Test]
        public async Task GetAsync_NoMatchingTerm_ShouldReturnNull()
        {
            // Arrange
            var terms = new[]
            {
                new Term { Content = "Term 1", LastUpdatedBy = "User1", LastUpdatedDate = DateTime.UtcNow }
            };

            _dbContext.Terms.AddRange(terms);
            await _dbContext.SaveChangesAsync();

            // Act
            var retrievedTerm = await _repository.GetAsync(t => t.LastUpdatedBy == "NonExistentUser");

            // Assert
            Assert.That(retrievedTerm, Is.Null);
        }
    }
}