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
    public class PrivacyPolicyRepositoryTests
    {
        private CursusDbContext _context;
        private PrivacyPolicyRepository _repository;
        private DbContextOptions<CursusDbContext> _options;

        [SetUp]
        public void Setup()
        {
            // Create in-memory database options
            _options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: "TestPrivacyPolicyDb")
                .Options;

            // Create a fresh context for each test
            _context = new CursusDbContext(_options);
            _repository = new PrivacyPolicyRepository(_context);

            // Ensure database is clean before each test
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task AddAsync_ShouldAddPrivacyPolicyToDatabase()
        {
            // Arrange
            var privacyPolicy = new PrivacyPolicy
            {
                Content = "Test Privacy Policy Content",
                LastUpdatedBy = "admin",
                LastUpdatedDate = DateTime.UtcNow
            };

            // Act
            var addedPrivacyPolicy = await _repository.AddAsync(privacyPolicy);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(addedPrivacyPolicy, Is.Not.Null);
            Assert.That(addedPrivacyPolicy.Id, Is.GreaterThan(0));
            Assert.That(addedPrivacyPolicy.Content, Is.EqualTo("Test Privacy Policy Content"));
            Assert.That(addedPrivacyPolicy.LastUpdatedBy, Is.EqualTo("admin"));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemovePrivacyPolicyFromDatabase()
        {
            // Arrange
            var privacyPolicy = new PrivacyPolicy
            {
                Content = "Delete Test Policy",
                LastUpdatedBy = "admin",
                LastUpdatedDate = DateTime.UtcNow
            };
            await _repository.AddAsync(privacyPolicy);
            await _context.SaveChangesAsync();

            // Act
            var deletedPrivacyPolicy = await _repository.DeleteAsync(privacyPolicy);
            await _context.SaveChangesAsync();

            // Assert
            var remainingPolicies = await _repository.GetAllAsync();
            Assert.That(remainingPolicies, Is.Empty);
            Assert.That(deletedPrivacyPolicy, Is.EqualTo(privacyPolicy));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllPrivacyPolicies()
        {
            // Arrange
            var policies = new[]
            {
                new PrivacyPolicy
                {
                    Content = "Policy 1",
                    LastUpdatedBy = "admin1",
                    LastUpdatedDate = DateTime.UtcNow
                },
                new PrivacyPolicy
                {
                    Content = "Policy 2",
                    LastUpdatedBy = "admin2",
                    LastUpdatedDate = DateTime.UtcNow.AddDays(1)
                }
            };
            await _repository.AddAsync(policies[0]);
            await _repository.AddAsync(policies[1]);
            await _context.SaveChangesAsync();

            // Act
            var retrievedPolicies = await _repository.GetAllAsync();

            // Assert
            Assert.That(retrievedPolicies.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllAsync_WithFilter_ShouldReturnFilteredPrivacyPolicies()
        {
            // Arrange
            var policies = new[]
            {
                new PrivacyPolicy
                {
                    Content = "Policy 1",
                    LastUpdatedBy = "admin1",
                    LastUpdatedDate = DateTime.UtcNow
                },
                new PrivacyPolicy
                {
                    Content = "Policy 2",
                    LastUpdatedBy = "admin2",
                    LastUpdatedDate = DateTime.UtcNow.AddDays(1)
                }
            };
            await _repository.AddAsync(policies[0]);
            await _repository.AddAsync(policies[1]);
            await _context.SaveChangesAsync();

            // Act
            var retrievedPolicies = await _repository.GetAllAsync(p => p.LastUpdatedBy == "admin1");

            // Assert
            Assert.That(retrievedPolicies.Count(), Is.EqualTo(1));
            Assert.That(retrievedPolicies.First().LastUpdatedBy, Is.EqualTo("admin1"));
        }

        [Test]
        public async Task GetAsync_ShouldReturnSpecificPrivacyPolicy()
        {
            // Arrange
            var privacyPolicy = new PrivacyPolicy
            {
                Content = "Specific Policy Content",
                LastUpdatedBy = "admin",
                LastUpdatedDate = DateTime.UtcNow
            };
            await _repository.AddAsync(privacyPolicy);
            await _context.SaveChangesAsync();

            // Act
            var retrievedPolicy = await _repository.GetAsync(p => p.LastUpdatedBy == "admin");

            // Assert
            Assert.That(retrievedPolicy, Is.Not.Null);
            Assert.That(retrievedPolicy.Content, Is.EqualTo("Specific Policy Content"));
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdatePrivacyPolicyInDatabase()
        {
            // Arrange
            var privacyPolicy = new PrivacyPolicy
            {
                Content = "Original Policy",
                LastUpdatedBy = "admin1",
                LastUpdatedDate = DateTime.UtcNow
            };
            await _repository.AddAsync(privacyPolicy);
            await _context.SaveChangesAsync();

            // Act
            privacyPolicy.Content = "Updated Policy";
            privacyPolicy.LastUpdatedBy = "admin2";
            var updatedPolicy = await _repository.UpdateAsync(privacyPolicy);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(updatedPolicy.Content, Is.EqualTo("Updated Policy"));
            Assert.That(updatedPolicy.LastUpdatedBy, Is.EqualTo("admin2"));
        }

        [Test]
        public async Task GetAsync_WithNonExistentFilter_ShouldReturnNull()
        {
            // Arrange
            var privacyPolicy = new PrivacyPolicy
            {
                Content = "Test Policy",
                LastUpdatedBy = "admin",
                LastUpdatedDate = DateTime.UtcNow
            };
            await _repository.AddAsync(privacyPolicy);
            await _context.SaveChangesAsync();

            // Act
            var retrievedPolicy = await _repository.GetAsync(p => p.LastUpdatedBy == "nonexistent");

            // Assert
            Assert.That(retrievedPolicy, Is.Null);
        }

        [Test]
        public async Task GetAsync_WithMultiplePolicies_ShouldReturnFirstMatchingPolicy()
        {
            // Arrange
            var policies = new[]
            {
                new PrivacyPolicy
                {
                    Content = "Policy 1",
                    LastUpdatedBy = "admin1",
                    LastUpdatedDate = DateTime.UtcNow
                },
                new PrivacyPolicy
                {
                    Content = "Policy 2",
                    LastUpdatedBy = "admin1",
                    LastUpdatedDate = DateTime.UtcNow.AddDays(1)
                }
            };
            await _repository.AddAsync(policies[0]);
            await _repository.AddAsync(policies[1]);
            await _context.SaveChangesAsync();

            // Act
            var retrievedPolicy = await _repository.GetAsync(p => p.LastUpdatedBy == "admin1");

            // Assert
            Assert.That(retrievedPolicy, Is.Not.Null);
            Assert.That(retrievedPolicy.Content, Is.EqualTo("Policy 1"));
        }
    }
}