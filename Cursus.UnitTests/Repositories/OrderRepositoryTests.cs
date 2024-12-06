using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cursus.Repository.Repository;
using Moq;
using Cursus.Data.Enums;
using Cursus.Data.DTO;

namespace Cursus.UnitTests.Repositories
{
    public class OrderRepositoryTests
    {
        private CursusDbContext _dbContext;
        private OrderRepository _orderRepository;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: "CursusTestDb")
                .Options;

            _dbContext = new CursusDbContext(options);
            _orderRepository = new OrderRepository(_dbContext);
        }
        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
        [Test]
        public async Task GetOrderWithCartAndItemsAsync_OrderExists_ReturnsOrderWithCartAndItems()
        {
            // Arrange
            var cart = new Cart { CartId = 1, UserId = "user1" };
            var order = new Order
            {
                OrderId = 1,
                CartId = cart.CartId,
                Amount = 100,
                DateCreated = DateTime.Now,
                Status = OrderStatus.Paid,
                Cart = cart
            };
            _dbContext.Order.Add(order);
            _dbContext.Cart.Add(cart);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderRepository.GetOrderWithCartAndItemsAsync(order.OrderId);

            // Assert
            Assert.That(result,Is.Not.Null);
            Assert.That(result.OrderId,Is.EqualTo(order.OrderId));
            Assert.That(result.CartId,Is.EqualTo(cart.CartId));
        }

        [Test]
        public async Task GetOrderWithCartAndItemsAsync_OrderDoesNotExist_ReturnsNull()
        {
            // Act
            var result = await _orderRepository.GetOrderWithCartAndItemsAsync(999);

            // Assert
            Assert.That(result,Is.Null);
        }

        [Test]
        public async Task UpdateOrderStatus_OrderExists_UpdatesStatus()
        {
            // Arrange
            var order = new Order
            {
                OrderId = 1,
                Amount = 100,
                Status = OrderStatus.PendingPayment,
                DateCreated = DateTime.Now
            };
            _dbContext.Order.Add(order);
            await _dbContext.SaveChangesAsync();

            // Act
            await _orderRepository.UpdateOrderStatus(order.OrderId, OrderStatus.Paid);

            // Assert
            var updatedOrder = await _dbContext.Order.FindAsync(order.OrderId);
            Assert.That(updatedOrder.Status,Is.EqualTo(OrderStatus.Paid ));
        }

        [Test]
        public async Task UpdateOrderStatus_OrderDoesNotExist_DoesNotThrowException()
        {
            // Act
            await _orderRepository.UpdateOrderStatus(999, OrderStatus.Paid);

            // Assert
            // No exception is expected
        }

        [Test]
        public async Task GetOrderHistory_UserHasOrders_ReturnsOrders()
        {
            // Arrange
            var userId = "user1";
            var cart1 = new Cart { CartId = 1, UserId = userId };
            var cart2 = new Cart { CartId = 2, UserId = userId };
            var order1 = new Order
            {
                OrderId = 1,
                CartId = cart1.CartId,
                Amount = 100,
                Status = OrderStatus.Paid,
                DateCreated = DateTime.Now,
                Cart = cart1
            };
            var order2 = new Order
            {
                OrderId = 2,
                CartId = cart2.CartId,
                Amount = 200,
                Status = OrderStatus.PendingPayment,
                DateCreated = DateTime.Now,
                Cart = cart2
            };
            _dbContext.Cart.AddRange(cart1, cart2);
            _dbContext.Order.AddRange(order1, order2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderRepository.GetOrderHistory(userId);

            // Assert
            Assert.That( result.Count,Is.EqualTo(2));
        }




        [Test]
        public async Task GetTotalOrderStatus_ReturnsCorrectCount()
        {
            // Arrange
            var order1 = new Order
            {
                OrderId = 1,
                Status = OrderStatus.Paid,
                DateCreated = DateTime.Now
            };
            var order2 = new Order
            {
                OrderId = 2,
                Status = OrderStatus.PendingPayment,
                DateCreated = DateTime.Now.AddDays(-1)
            };
            _dbContext.Order.AddRange(order1, order2);
            await _dbContext.SaveChangesAsync();

            // Act
            var pendingOrders = await _orderRepository.GetTotalOrderStatus(null, null, OrderStatus.PendingPayment);
            var paidOrders = await _orderRepository.GetTotalOrderStatus(null, null, OrderStatus.Paid);

            // Assert
            Assert.That(pendingOrders, Is.EqualTo(1));
            Assert.That(paidOrders,Is.EqualTo(1));
        }

    }
}
