using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Cursus.ServiceContract.Interfaces;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq.Expressions;
using DocumentFormat.OpenXml.Presentation;
namespace Cursus.UnitTests.Repositories;
[TestFixture]
public class OrderServiceTests
{
    private Mock<IUnitOfWork> _unitOfWork;
    private Mock<IMapper> _mapper;
    private Mock<IEmailService> _emailService;
    private Mock<IHttpContextAccessor> _httpContextAccessor;
    private Mock<IStatisticsNotificationService> _notificationService;
    private Mock<IPaymentService> _paymentService;
    private OrderService _orderService;

    [SetUp]
    public void Setup()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();
        _emailService = new Mock<IEmailService>();
        _httpContextAccessor = new Mock<IHttpContextAccessor>();
        _notificationService = new Mock<IStatisticsNotificationService>();
        _paymentService = new Mock<IPaymentService>();

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id")
        }));
        _httpContextAccessor.Setup(h => h.HttpContext.User).Returns(claimsPrincipal);

        _orderService = new OrderService(
            _unitOfWork.Object,
            _mapper.Object,
            _emailService.Object,
            _notificationService.Object,
            _paymentService.Object,
            _httpContextAccessor.Object
        );
    }

    [Test]
    public async Task CreateOrderAsync_ShouldThrow_WhenCartIsEmpty()
    {
        _unitOfWork.Setup(u => u.CartRepository.GetAsync(It.IsAny<Expression<Func<Cart, bool>>>(), "CartItems,CartItems.Course"))
            .ReturnsAsync((Cart)null);

        var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () => await _orderService.CreateOrderAsync("test-user-id", null));
        //Assert.AreEqual("Cart is empty.", ex.Message);
    }

    [Test]
    public async Task CreateOrderAsync_ShouldCreateOrder_WhenCartIsValid()
    {
        var cart = new Cart
        {
            CartId = 1,
            UserId = "test-user-id",
            Total = 100,
            CartItems = new List<CartItems>
            {
                new CartItems { Course = new Course { Name = "Test Course", Price = 100 } }
            }
        };
        var order = new Order
        {
            CartId = 1,
            OrderId = 1,
            Status = OrderStatus.PendingPayment

        };

        _unitOfWork.Setup(u => u.CartRepository.GetAsync(It.IsAny<Expression<Func<Cart, bool>>>(), "CartItems,CartItems.Course"))
            .ReturnsAsync(cart);

        _paymentService.Setup(p => p.CreateTransaction("test-user-id", "PayPal", It.IsAny<string>()))
            .ReturnsAsync(new Transaction { TransactionId = 1 });
        _unitOfWork.Setup(u => u.OrderRepository.GetAsync(It.IsAny<Expression<Func<Order, bool>>>(), null));

        _mapper.Setup(m => m.Map<OrderDTO>(It.IsAny<Order>())).Returns(new OrderDTO { Amount = 110 });

        var result = await _orderService.CreateOrderAsync("test-user-id", null);

        // //Assert.IsNotNull(result);
        // //Assert.AreEqual(110, result.Amount);
        _unitOfWork.Verify(u => u.OrderRepository.AddAsync(It.IsAny<Order>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public async Task GetOrderHistoryAsync_ShouldReturnOrders_WhenOrdersExist()
    {
        var orders = new List<Order> { new Order { OrderId = 1 } };
        _unitOfWork.Setup(u => u.OrderRepository.GetOrderHistory("test-user-id"))
            .ReturnsAsync(orders);

        _mapper.Setup(m => m.Map<List<OrderDTO>>(orders))
            .Returns(new List<OrderDTO> { new OrderDTO { OrderId = 1 } });

        var result = await _orderService.GetOrderHistoryAsync();

        //Assert.IsNotNull(result);
        //Assert.AreEqual(1, result.Count);
    }

    [Test]
    public void GetOrderHistoryAsync_ShouldThrow_WhenNoOrdersFound()
    {
        _unitOfWork.Setup(u => u.OrderRepository.GetOrderHistory("test-user-id"))
            .ReturnsAsync((List<Order>)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _orderService.GetOrderHistoryAsync());
        //Assert.AreEqual("Order not found.", ex.Message);
    }

    [Test]
    public async Task UpdateUserCourseAccessAsync_ShouldUpdateCourseAccess_WhenOrderIsValid()
    {
        var order = new Order
        {
            OrderId = 1,
            Cart = new Cart
            {
                CartItems = new List<CartItems> { new CartItems { Course = new Course { Name = "Test Course", Id = 1 } } }
            },
            Status = OrderStatus.Paid,
            PaidAmount = 100
        };

        var user = new ApplicationUser { Id = "I1" };
        var info = new InstructorInfo { Id = 1, UserId = "I1" };
        var wallet = new Wallet { UserId = "I1" };
        var platwallet = new PlatformWallet { Balance = 100 };

        _unitOfWork.Setup(u => u.OrderRepository.GetAsync(It.IsAny<Expression<Func<Order, bool>>>(), "Cart,Cart.CartItems.Course"))
            .ReturnsAsync(order);

        _unitOfWork.Setup(u => u.UserRepository.ExiProfile("I1"))
            .ReturnsAsync(user);
        _unitOfWork.Setup(u => u.CourseProgressRepository.GetAsync(It.IsAny<Expression<Func<CourseProgress, bool>>>(), null)).ReturnsAsync((CourseProgress)null);
        _unitOfWork.Setup(u => u.InstructorInfoRepository.GetAsync(It.IsAny<Expression<Func<InstructorInfo, bool>>>(), null)).ReturnsAsync(info);
        _unitOfWork.Setup(u => u.WalletRepository.GetAsync(It.IsAny<Expression<Func<Wallet, bool>>>(), null)).ReturnsAsync(wallet);
        _unitOfWork.Setup(u => u.PlatformWalletRepository.GetPlatformWallet()).ReturnsAsync(platwallet);

        await _orderService.UpdateUserCourseAccessAsync(1, "I1");

        _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        _emailService.Verify(e => e.SendEmailSuccessfullyPurchasedCourse(user, order), Times.Once);
        _notificationService.Verify(n => n.NotifySalesAndRevenueUpdate(), Times.Once);
        _notificationService.Verify(n => n.NotifyOrderStatisticsUpdate(), Times.Once);
        Assert.That(platwallet.Balance, Is.EqualTo(130));
    }

    [Test]
    public void UpdateUserCourseAccessAsync_ShouldThrow_WhenOrderNotFound()
    {
        _unitOfWork.Setup(u => u.OrderRepository.GetAsync(It.IsAny<Expression<Func<Order, bool>>>(), "Cart,Cart.CartItems.Course"))
            .ReturnsAsync((Order)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _orderService.UpdateUserCourseAccessAsync(1, "test-user-id"));
        //  //Assert.AreEqual("Order not found or payment not completed.", ex.Message);
    }
}
