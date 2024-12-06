using NUnit.Framework;
using Moq;
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
    public class CategoryRepositoryTests
    {
        private CursusDbContext _context;
        private CategoryRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: $"CursusDb_{System.Guid.NewGuid()}")
                .Options;

            _context = new CursusDbContext(options);
            _repository = new CategoryRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddAsync_ShouldAddCategoryAndReturnIt()
        {
            // Arrange
            var category = new Category { Name = "Test Category", Description = "Test Description" };

            // Act
            var result = await _repository.AddAsync(category);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(await _context.Categories.CountAsync(), Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("Test Category"));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveCategoryAndReturnIt()
        {
            // Arrange
            var category = new Category { Name = "Test Category", Description = "Test Description" };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(category);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(await _context.Categories.CountAsync(), Is.EqualTo(0));
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateCategoryAndReturnIt()
        {
            // Arrange
            var category = new Category { Name = "Test Category", Description = "Test Description" };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            category.Name = "Updated Category";
            var result = await _repository.UpdateAsync(category);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Updated Category"));
            var updatedCategory = await _context.Categories.FirstAsync();
            Assert.That(updatedCategory.Name, Is.EqualTo("Updated Category"));
        }

        [Test]
        public async Task GetAsync_WithValidFilter_ShouldReturnCategory()
        {
            // Arrange
            var category = new Category { Name = "Test Category", Description = "Test Description" };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAsync(c => c.Name == "Test Category");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Test Category"));
        }

        [Test]
        public async Task GetAsync_WithInvalidFilter_ShouldReturnNull()
        {
            // Arrange
            var category = new Category { Name = "Test Category", Description = "Test Description" };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAsync(c => c.Name == "Non-existent Category");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_WithNoFilter_ShouldReturnAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Name = "Category 1", Description = "Description 1" },
                new Category { Name = "Category 2", Description = "Description 2" },
                new Category { Name = "Category 3", Description = "Description 3" }
            };
            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task GetAllAsync_WithFilter_ShouldReturnFilteredCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Name = "Category 1", Description = "Test" },
                new Category { Name = "Category 2", Description = "Other" },
                new Category { Name = "Category 3", Description = "Test" }
            };
            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync(c => c.Description == "Test");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task AnyAsync_WithExistingCategory_ShouldReturnTrue()
        {
            // Arrange
            var category = new Category { Name = "Test Category", Description = "Test Description" };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.AnyAsync(c => c.Name == "Test Category");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task AnyAsync_WithNonExistingCategory_ShouldReturnFalse()
        {
            // Arrange
            var category = new Category { Name = "Test Category", Description = "Test Description" };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.AnyAsync(c => c.Name == "Non-existent Category");

            // Assert
            Assert.That(result, Is.False);
        }
    }
}