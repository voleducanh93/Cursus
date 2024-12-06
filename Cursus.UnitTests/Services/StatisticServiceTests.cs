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
using Cursus.Data.Enums;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Cursus.ServiceContract.Interfaces;
using System.Linq.Expressions;
namespace Cursus.UnitTests.Services;
[TestFixture]
public class StatisticServiceTests
{
    private Mock<IMapper> _mapperMock;
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<IOrderService> _orderServiceMock;
    private Mock<IInstructorService> _instructorServiceMock;
    private StatisticService _service;

    [SetUp]
    public void Setup()
    {
        _mapperMock = new Mock<IMapper>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderServiceMock = new Mock<IOrderService>();
        _instructorServiceMock = new Mock<IInstructorService>();

        _service = new StatisticService(
            _mapperMock.Object,
            _unitOfWorkMock.Object,
            _orderServiceMock.Object,
            _instructorServiceMock.Object
        );
    }

    [Test]
    public async Task GetMonthlyStatisticsAsync_ReturnsCorrectData()
    {
        // Arrange
        var expectedStats = (10, 0);
        _unitOfWorkMock
            .Setup(u => u.OrderRepository.GetDashboardMetricsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync((expectedStats, expectedStats));

        // Act
        var result = await _service.GetMonthlyStatisticsAsync();

        // Assert
        Assert.That(result,Is.Not.Null);
        Assert.That(result.Count,Is.EqualTo(12));
        Assert.That(result.Last().Month,Is.EqualTo("Nov 2024")); // Adjust based on the current date
    }

    [Test]
    public async Task GetOrderStatisticsAsync_ReturnsCorrectData()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.OrderRepository.GetTotalOrderStatus(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), OrderStatus.Paid))
            .ReturnsAsync(5);
        _unitOfWorkMock.Setup(u => u.OrderRepository.GetTotalOrderStatus(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), OrderStatus.Failed))
            .ReturnsAsync(3);
        _unitOfWorkMock.Setup(u => u.OrderRepository.GetTotalOrderStatus(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), OrderStatus.PendingPayment))
            .ReturnsAsync(2);

        // Act
        var result = await _service.GetOrderStatisticsAsync(DateTime.Now.AddMonths(-1), DateTime.Now);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.TotalOrder, Is.EqualTo(10));
        Assert.That(result.PaidedOrder, Is.EqualTo(5));
        Assert.That(result.FailedOrder, Is.EqualTo(3));
        Assert.That(result.PendingOrder, Is.EqualTo(2));
    }

    [Test]
    public async Task GetCourseStatisticsAsync_ReturnsCorrectData()
    {
        // Arrange
        var courses = new List<Course>
        {
            new Course { Id = 1, Status = true, IsApprove = ApproveStatus.Approved },
            new Course { Id = 2, Status = false, IsApprove = ApproveStatus.Approved }
        };
        _unitOfWorkMock.Setup(u => u.CourseRepository.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(),null)).ReturnsAsync(courses);

        // Act
        var result = await _service.GetCourseStatisticsAsync();

        // Assert
        Assert.That(result.totalCourses, Is.EqualTo(2));
        Assert.That(result.activeCourses, Is.EqualTo(1));
        Assert.That(result.inactiveCourses, Is.EqualTo(1));
    }

    [Test]
    public async Task GetTopInstructorsByRevenueAsync_ReturnsCorrectData()
    {
        // Arrange
        var instructors = new List<InstructorStatisticDTO>
        {
            new InstructorStatisticDTO { InstructorName = "Instructor1", TotalEarnings = 1000 },
            new InstructorStatisticDTO { InstructorName = "Instructor2", TotalEarnings = 800 }
        };

        _unitOfWorkMock
            .Setup(u => u.OrderRepository.GetTopInstructorsByRevenueAsync(2, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), 1, 10))
            .ReturnsAsync(instructors);

        // Act
        var result = await _service.GetTopInstructorsByRevenueAsync(2, DateTime.Now.AddMonths(-1), DateTime.Now, 1, 10);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.First().InstructorName, Is.EqualTo("Instructor1"));
    }

    [Test]
    public async Task GetStatisticsAsync_ReturnsCorrectData()
    {
        // Arrange
        var currentMetrics = (10, 1000m);
        var previousMetrics = (8, 800m);

        _unitOfWorkMock
            .Setup(u => u.OrderRepository.GetDashboardMetricsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync((currentMetrics, previousMetrics));

        // Act
        var result = await _service.GetStatisticsAsync(DateTime.Now.AddMonths(-1), DateTime.Now);

        // Assert
        Assert.That(result.totalSales, Is.EqualTo(10));
        Assert.That(result.salesChangePercentage, Is.EqualTo(25)); // (10-8)/8 * 100
        Assert.That(result.totalRevenue, Is.EqualTo(1000m));
        Assert.That(result.revenueChangePercentage, Is.EqualTo(25)); // (1000-800)/800 * 100
    }
}
