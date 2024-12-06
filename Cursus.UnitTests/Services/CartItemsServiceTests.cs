using AutoMapper;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Moq;
namespace Cursus.UnitTests.Services
{

    [TestFixture]
    public class CartItemsServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IMapper> _mapperMock;
        private CartItemsService _cartItemsService;

        [SetUp]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _cartItemsService = new CartItemsService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Test]
        public async Task DeleteCartItem_ShouldReturnTrue_WhenCartItemExists()
        {
            // Arrange
            var cartItemId = 1;
            var cartItem = new CartItems { CartItemsId = cartItemId };
            _unitOfWorkMock.Setup(u => u.CartItemsRepository.GetItemByID(cartItemId))
                           .ReturnsAsync(cartItem);
            _unitOfWorkMock.Setup(u => u.CartItemsRepository.DeleteCartItems(cartItem))
                           .Returns(Task.FromResult(true));
            _unitOfWorkMock.Setup(u => u.SaveChanges())
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _cartItemsService.DeleteCartItem(cartItemId);

            // Assert
            Assert.That(result, Is.True);
            _unitOfWorkMock.Verify(u => u.CartItemsRepository.DeleteCartItems(cartItem), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Test]
        public async Task DeleteCartItem_ShouldReturnFalse_WhenCartItemDoesNotExist()
        {
            // Arrange
            var cartItemId = 1;
            _unitOfWorkMock.Setup(u => u.CartItemsRepository.GetItemByID(cartItemId))
                           .ReturnsAsync((CartItems?)null);

            // Act
            var result = await _cartItemsService.DeleteCartItem(cartItemId);

            // Assert
            Assert.That(result, Is.False);
            _unitOfWorkMock.Verify(u => u.CartItemsRepository.DeleteCartItems(It.IsAny<CartItems>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Never);
        }

        [Test]
        public async Task DeleteCartItem_ShouldThrowException_WhenDeleteFails()
        {
            // Arrange
            var cartItemId = 1;
            var cartItem = new CartItems { CartItemsId = cartItemId };
            _unitOfWorkMock.Setup(u => u.CartItemsRepository.GetItemByID(cartItemId))
                           .ReturnsAsync(cartItem);
            _unitOfWorkMock.Setup(u => u.CartItemsRepository.DeleteCartItems(cartItem))
                           .ThrowsAsync(new Exception("Deletion failed"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _cartItemsService.DeleteCartItem(cartItemId));
        }

        [Test]
        public async Task GetAllCartItems_ShouldReturnCartItems_WhenCartItemsExist()
        {
            // Arrange
            var cartId = 1;
            var cartItems = new List<CartItems>
        {
            new CartItems { CartItemsId = 1, CartId = cartId },
            new CartItems { CartItemsId = 2, CartId = cartId }
        };
            _unitOfWorkMock.Setup(u => u.CartItemsRepository.GetAllItems(cartId))
                           .ReturnsAsync(cartItems);

            // Act
            var result = await _cartItemsService.GetAllCartItems(cartId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            _unitOfWorkMock.Verify(u => u.CartItemsRepository.GetAllItems(cartId), Times.Once);
        }

        [Test]
        public async Task GetAllCartItems_ShouldReturnEmptyList_WhenNoCartItemsExist()
        {
            // Arrange
            var cartId = 1;
            _unitOfWorkMock.Setup(u => u.CartItemsRepository.GetAllItems(cartId))
                           .ReturnsAsync(new List<CartItems>());

            // Act
            var result = await _cartItemsService.GetAllCartItems(cartId);

            // Assert
            Assert.That(result, Is.Empty);
            _unitOfWorkMock.Verify(u => u.CartItemsRepository.GetAllItems(cartId), Times.Once);
        }

        [Test]
        public async Task GetAllCartItems_ShouldThrowException_WhenGetAllItemsFails()
        {
            // Arrange
            var cartId = 1;
            _unitOfWorkMock.Setup(u => u.CartItemsRepository.GetAllItems(cartId))
                           .ThrowsAsync(new Exception("Retrieval failed"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _cartItemsService.GetAllCartItems(cartId));
        }
    }
}