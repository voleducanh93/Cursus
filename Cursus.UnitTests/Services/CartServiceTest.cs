using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Cursus.Data.Entities;
using Cursus.Repository.Repository;
using Cursus.Service;
using Cursus.Data.DTO;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Microsoft.AspNetCore.Http;
namespace Cursus.UnitTests.Services;
using System.Linq.Expressions;
[TestFixture]
public class CartServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<IMapper> _mapperMock;
    private CartService _service;

    [SetUp]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _service = new CartService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task DeleteCart_ReturnsFalse_WhenCartNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.CartRepository.GetCartByID(1))
            .ReturnsAsync((Cart)null);

        // Act
        var result = await _service.DeleteCart(1);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteCart_ReturnsTrue_WhenCartDeleted()
    {
        // Arrange
        var cart = new Cart { CartId = 1 };
        _unitOfWorkMock.Setup(u => u.CartRepository.GetCartByID(1)).ReturnsAsync(cart);
        _unitOfWorkMock.Setup(u => u.CartRepository.DeleteCart(cart)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteCart(1);

        // Assert
        Assert.That(result, Is.True);
        _unitOfWorkMock.Verify(u => u.CartRepository.DeleteCart(cart), Times.Once);
    }

    [Test]
    public async Task GetAllCart_ReturnsCarts()
    {
        // Arrange
        var carts = new List<Cart>
        {
            new Cart { CartId = 1, UserId = "user1" },
            new Cart { CartId = 2, UserId = "user2" }
        };

        _unitOfWorkMock.Setup(u => u.CartRepository.GetCart()).ReturnsAsync(carts);

        // Act
        var result = await _service.GetAllCart();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetCartByID_ReturnsCart()
    {
        // Arrange
        var cart = new Cart { CartId = 1, UserId = "user1" };
        _unitOfWorkMock.Setup(u => u.CartRepository.GetCartByID(1)).ReturnsAsync(cart);

        // Act
        var result = await _service.GetCartByID(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CartId, Is.EqualTo(1));
    }

    [Test]
    public async Task AddCourseToCartAsync_AddsCourseToCart()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user1" };
        var course = new Course { Id = 1, Price = 100 };
        var cart = new Cart { CartId = 1, UserId = "user1", CartItems = new List<CartItems>(), Total = 0 };

        _unitOfWorkMock.Setup(u => u.UserRepository.ExiProfile("user1")).ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(), null)).ReturnsAsync(course);
        _unitOfWorkMock.Setup(u => u.CourseProgressRepository.GetAsync(It.IsAny<Expression<Func<CourseProgress, bool>>>(), null)).ReturnsAsync((CourseProgress)null);
        _unitOfWorkMock.Setup(u => u.CartRepository.GetAsync(It.IsAny<Expression<Func<Cart, bool>>>(), "CartItems")).ReturnsAsync(cart);

        // Act
        await _service.AddCourseToCartAsync(1, "user1");

        // Assert
        Assert.That(cart.CartItems.Count, Is.EqualTo(1));
        Assert.That(cart.Total, Is.EqualTo(100));
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public void AddCourseToCartAsync_ThrowsKeyNotFoundException_WhenUserNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.UserRepository.ExiProfile("user1")).ReturnsAsync((ApplicationUser)null);

        // Act & Assert
        Assert.That(async () => await _service.AddCourseToCartAsync(1, "user1"), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void AddCourseToCartAsync_ThrowsKeyNotFoundException_WhenCourseNotFound()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user1" };
        _unitOfWorkMock.Setup(u => u.UserRepository.ExiProfile("user1")).ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(), null)).ReturnsAsync((Course)null);

        // Act & Assert
        Assert.That(async () => await _service.AddCourseToCartAsync(1, "user1"), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void AddCourseToCartAsync_ThrowsBadHttpRequestException_WhenCourseAlreadyPurchased()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user1" };
        var course = new Course { Id = 1, Price = 100 };
        var purchasedCourse = new CourseProgress { Course = course };
        _unitOfWorkMock.Setup(u => u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(), null)).ReturnsAsync(course);
        _unitOfWorkMock.Setup(u => u.UserRepository.ExiProfile("user1")).ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.CourseProgressRepository.GetAsync(It.IsAny<Expression<Func<CourseProgress, bool>>>(), null)).ReturnsAsync(purchasedCourse);

        // Act & Assert
        Assert.That(async () => await _service.AddCourseToCartAsync(1, "user1"), Throws.TypeOf<BadHttpRequestException>());
    }

    [Test]
    public async Task GetCartByUserIdAsync_ReturnsMappedCart()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user1" };
        var cart = new Cart { CartId = 1, UserId = "user1", CartItems = new List<CartItems>() };
        var cartDTO = new CartDTO {UserId = "user1", CartItems = new List<CartItemsDTO>() };

        _unitOfWorkMock.Setup(u => u.UserRepository.ExiProfile("user1")).ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.CartRepository.GetAsync(It.IsAny<Expression<Func<Cart, bool>>>(), "CartItems,CartItems.Course"))
            .ReturnsAsync(cart);
        _mapperMock.Setup(m => m.Map<CartDTO>(cart)).Returns(cartDTO);

        // Act
        var result = await _service.GetCartByUserIdAsync("user1");

        // Assert
        Assert.That(result, Is.Not.Null);
    }
}
