using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.DTO.Category;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class CategoryServiceTests
    {
        private CategoryService _categoryService;
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IMapper> _mockMapper;

        [SetUp]
        public void SetUp()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            _categoryService = new CategoryService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Test]
        public async Task GetCategoriesAsync_ShouldReturnPagedCategories_WhenCategoriesExist()
        {
            // Arrange
            var categories = new List<Category>
            {
new Category { Id = 1, Name = "Test Category 1", Description = "Test Description 1" },
                new Category { Id = 2, Name = "Test Category 2", Description = "Test Description 2" }
            };

            // Mock the repository method to return the categories as an IQueryable
            _mockUnitOfWork.Setup(u => u.CategoryRepository.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<string>()))
               .ReturnsAsync(categories.AsQueryable());

            var categoryDTOs = new List<CategoryDTO>
            {
                new CategoryDTO { Id = 1, Name = "Test Category 1", Description = "Test Description 1" },
                new CategoryDTO { Id = 2, Name = "Test Category 2", Description = "Test Description 2" }
            };

            _mockMapper.Setup(m => m.Map<List<CategoryDTO>>(It.IsAny<List<Category>>()))
                .Returns(categoryDTOs);

            // Act
            var result = await _categoryService.GetCategoriesAsync(null, null, null, 1, 2);

            // Assert
            Assert.That(result, Is.Not.Null);
            //Assert.AreEqual(2, result.Items.Count);
            Assert.That(result.Items.Count, Is.EqualTo(2));
            Assert.That(result.Items[0].Name, Is.EqualTo("Test Category 1"));
        }

        [Test]
        public async Task GetCategoryById_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test Category", Description = "Test Description" };

            _mockUnitOfWork.Setup(u => u.CategoryRepository.GetAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string>()))
               .ReturnsAsync(category);

            var categoryDTO = new CategoryDTO { Id = 1, Name = "Test Category", Description = "Test Description" };

            _mockMapper.Setup(m => m.Map<CategoryDTO>(category)).Returns(categoryDTO);

            // Act
            var result = await _categoryService.GetCategoryById(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("Test Category"));
        }

        [Test]
        public async Task CreateCategory_ShouldReturnCategoryDTO_WhenCategoryIsCreated()
        {
            // Arrange
            var createCategoryDTO = new CreateCategoryDTO { Name = "New Category", Description = "New Description" };
            var newCategory = new Category { Id = 1, Name = "New Category", Description = "New Description" };

            _mockUnitOfWork.Setup(u => u.CategoryRepository.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<Category>(createCategoryDTO)).Returns(newCategory);
            _mockUnitOfWork.Setup(u => u.CategoryRepository.AddAsync(newCategory)).Returns(Task.FromResult(newCategory));
            _mockUnitOfWork.Setup(u => u.SaveChanges()).Returns(Task.CompletedTask);

            var categoryDTO = new CategoryDTO
            {
                Id = newCategory.Id,
                Name = newCategory.Name,
                Description = newCategory.Description
            };

            // Act
            var result = await _categoryService.CreateCategory(createCategoryDTO);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("New Category"));
        }

        [Test]
        public async Task CreateCategory_ShouldThrowBadHttpRequestException_WhenCategoryNameExists()
        {
            // Arrange
            var createCategoryDTO = new CreateCategoryDTO { Name = "Existing Category", Description = "Existing Description" };

            // Mock the repository to return true, indicating the category name already exists
            _mockUnitOfWork.Setup(u => u.CategoryRepository.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(true);

            // Act & Assert: Check for BadHttpRequestException instead of a generic Exception
            var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () => await _categoryService.CreateCategory(createCategoryDTO));

            Assert.That(ex.Message, Is.EqualTo("A category with this name already exists."));
        }
        [Test]
        public async Task DeleteCategory_ShouldReturnTrue_WhenCategoryIsDeleted()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test Category" };

            _mockUnitOfWork.Setup(u => u.CategoryRepository.GetAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string>()))
               .ReturnsAsync(category);
            _mockUnitOfWork.Setup(u => u.CategoryRepository.DeleteAsync(category)).Returns(Task.FromResult(category));
            _mockUnitOfWork.Setup(u => u.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            var result = await _categoryService.DeleteCategory(1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task DeleteCategory_ShouldThrowKeyNotFoundException_WhenCategoryNotFound()
        {
            // Arrange: Mock the repository to return null, indicating the category was not found
            _mockUnitOfWork.Setup(u => u.CategoryRepository.GetAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string>()))
               .ReturnsAsync((Category)null);

            // Act & Assert: Check for KeyNotFoundException instead of a generic Exception
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _categoryService.DeleteCategory(1));
            Assert.That(ex.Message, Is.EqualTo("Category not found."));
        }
    }
}
