using NUnit.Framework;
using Moq;
using Microsoft.EntityFrameworkCore;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cursus.Repository.Repository;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class CartRepositoryTests
    {
        private Mock<CursusDbContext> _mockContext;
        private CartRepository _cartRepository;
        private Mock<DbSet<Cart>> _mockCartDbSet;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<CursusDbContext>();
            _mockCartDbSet = new Mock<DbSet<Cart>>();
            _mockContext.Setup(c => c.Cart).Returns(_mockCartDbSet.Object);
            _cartRepository = new CartRepository(_mockContext.Object);
        }

        [Test]
        public async Task DeleteCart_ValidCart_ReturnsTrue()
        {
            // Arrange
            var cart = new Cart { CartId = 1, IsPurchased = false };

            // Act
            var result = await _cartRepository.DeleteCart(cart);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(cart.IsPurchased, Is.True);
        }

        [Test]
        public async Task UpdateIsPurchased_ExistingCart_UpdatesIsPurchased()
        {
            // Arrange
            var cart = new Cart { CartId = 1, IsPurchased = false };
            _mockContext.Setup(c => c.Cart.FindAsync(1))
                .ReturnsAsync(cart);

            // Act
            await _cartRepository.UpdateIsPurchased(1, true);

            // Assert
            Assert.That(cart.IsPurchased, Is.True);
            _mockContext.Verify(c => c.Cart.Update(cart), Times.Once);
        }

        [Test]
        public async Task UpdateIsPurchased_NonExistingCart_DoesNotUpdate()
        {
            // Arrange
            _mockContext.Setup(c => c.Cart.FindAsync(1))
                .ReturnsAsync((Cart)null);

            // Act
            await _cartRepository.UpdateIsPurchased(1, true);

            // Assert
            _mockContext.Verify(c => c.Cart.Update(It.IsAny<Cart>()), Times.Never);
        }

        //[Test]
        //public async Task GetCart_ReturnsAllCarts()
        //{
        //    // Arrange
        //    var carts = new List<Cart>
        //    {
        //        new Cart { CartId = 1, IsPurchased = false },
        //        new Cart { CartId = 2, IsPurchased = true }
        //    };

        //    var mockDbSet = GetMockDbSet(carts);
        //    _mockContext.Setup(c => c.Set<Cart>()).Returns(mockDbSet.Object);

        //    // Act
        //    var result = await _cartRepository.GetCart();

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result.Count(), Is.EqualTo(2));
        //}

        //[Test]
        //public async Task GetCartByID_ExistingCart_ReturnsCart()
        //{
        //    // Arrange
        //    var cart = new Cart { CartId = 1, IsPurchased = false };
        //    var carts = new List<Cart> { cart };

        //    var mockDbSet = GetMockDbSet(carts);
        //    _mockContext.Setup(c => c.Set<Cart>()).Returns(mockDbSet.Object);

        //    // Act
        //    var result = await _cartRepository.GetCartByID(1);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result.CartId, Is.EqualTo(1));
        //}

        //[Test]
        //public async Task GetCartByID_NonExistingCart_ReturnsNull()
        //{
        //    // Arrange
        //    var carts = new List<Cart>();
        //    var mockDbSet = GetMockDbSet(carts);
        //    _mockContext.Setup(c => c.Set<Cart>()).Returns(mockDbSet.Object);

        //    // Act
        //    var result = await _cartRepository.GetCartByID(1);

        //    // Assert
        //    Assert.That(result, Is.Null);
        //}

        private Mock<DbSet<T>> GetMockDbSet<T>(IList<T> entities) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            var queryable = entities.AsQueryable();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            return mockSet;
        }
    }
}