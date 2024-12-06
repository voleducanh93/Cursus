using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Cursus.Data.Entities;
using Cursus.Repository.Repository;
using Cursus.Service;
using Cursus.Data.DTO;
using Microsoft.EntityFrameworkCore;
using Cursus.Data.Enums;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Cursus.ServiceContract.Interfaces;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq.Expressions;
namespace Cursus.UnitTests.Repositories;
[TestFixture]
public class PayoutRequestServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<IEmailService> _emailServiceMock;
    private Mock<IMapper> _mapperMock;
    private Mock<DbSet<PayoutRequest>> _payoutRequestDbSetMock;
    private Mock<DbSet<Transaction>> _transactionDbSetMock;
    private Mock<DbSet<ApplicationUser>> _userDbSetMock;
    private CursusDbContext _dbContext;
    private PayoutRequestService _service;
    private Mock<IAdminService> _adminServiceMock;

    [SetUp]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailServiceMock = new Mock<IEmailService>();
        _mapperMock = new Mock<IMapper>();
        _adminServiceMock = new Mock<IAdminService>();

        var options = new DbContextOptionsBuilder<CursusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new CursusDbContext(options);

        _service = new PayoutRequestService(
            _unitOfWorkMock.Object,
            _dbContext,
            _mapperMock.Object,
            _emailServiceMock.Object,
            _adminServiceMock.Object
        );
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task GetApprovedPayoutRequest_ReturnsApprovedRequests()
    {
        // Arrange
        var payoutRequest = new PayoutRequest { Id = 1, PayoutRequestStatus = PayoutRequestStatus.Approved, TransactionId = 1 };
        var transaction = new Transaction { TransactionId = 1, Amount = 100, UserId = "1" };
        var user = new ApplicationUser { Id = "1", UserName = "TestUser" };

        _dbContext.PayoutRequests.Add(payoutRequest);
        _dbContext.Transactions.Add(transaction);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetApprovedPayoutRequest();

        // Assert
        Assert.That(result,Is.Not.Null);
        Assert.That(result.Count,Is.EqualTo(1));
        //Assert.That(result[0].InstructorName,Is.EqualTo("TestUser"));
    }

    [Test]
    public async Task AcceptPayout_ApprovesPayoutRequest()
    {
        // Arrange
        var payoutRequest = new PayoutRequest { Id = 1, PayoutRequestStatus = PayoutRequestStatus.Pending, TransactionId = 1, InstructorId = 1, Transaction = new Transaction {Amount=100 } };
        var transaction = new Transaction { TransactionId = 1, Amount = 100 };
        var instructor = new InstructorInfo { Id = 1, TotalWithdrawn = 0, User = new ApplicationUser { Email = "test@example.com" } };

        _unitOfWorkMock.Setup(u => u.PayoutRequestRepository.GetPayoutByID(1)).ReturnsAsync(payoutRequest);
        _unitOfWorkMock.Setup(u => u.InstructorInfoRepository.GetAsync(It.IsAny<Expression<Func<InstructorInfo, bool>>>(), "User")).ReturnsAsync(instructor);
        _mapperMock.Setup(u => u.Map<PayoutAcceptDTO>(payoutRequest)).Returns(new PayoutAcceptDTO { });

        // Act
        var result = await _service.AcceptPayout(1);

        // Assert
        Assert.That(payoutRequest.PayoutRequestStatus, Is.EqualTo(PayoutRequestStatus.Approved));
        Assert.That(instructor.TotalWithdrawn, Is.EqualTo(100));
        _emailServiceMock.Verify(e => e.SendEmail(It.IsAny<EmailRequestDTO>()), Times.Once);
    }

    [Test]
    public async Task DenyPayout_RejectsPayoutRequest()
    {
        // Arrange
        var payoutRequest = new PayoutRequest { Id = 1, PayoutRequestStatus = PayoutRequestStatus.Pending, InstructorId = 1 };
        var instructor = new InstructorInfo { Id = 1, User = new ApplicationUser { Email = "test@example.com" } };

        _unitOfWorkMock.Setup(u => u.PayoutRequestRepository.GetPayoutByID(1)).ReturnsAsync(payoutRequest);
        _unitOfWorkMock.Setup(u => u.InstructorInfoRepository.GetAsync(It.IsAny<Expression<Func<InstructorInfo, bool>>>(), "User")).ReturnsAsync(instructor);
        _mapperMock.Setup(u => u.Map<PayoutDenyDTO>(payoutRequest)).Returns(new PayoutDenyDTO { });

        // Act
        var result = await _service.DenyPayout(1, "Invalid details");

        // Assert
        Assert.That(payoutRequest.PayoutRequestStatus, Is.EqualTo(PayoutRequestStatus.Rejected));
        Assert.That(result.Reason, Is.EqualTo("Invalid details"));
        _emailServiceMock.Verify(e => e.SendEmail(It.IsAny<EmailRequestDTO>()), Times.Once);
    }

    [Test]
    public void AcceptPayout_ThrowsIfRequestNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.PayoutRequestRepository.GetPayoutByID(1)).ReturnsAsync((PayoutRequest)null);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.AcceptPayout(1));
    }

    [Test]
    public void DenyPayout_ThrowsIfRequestNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.PayoutRequestRepository.GetPayoutByID(1)).ReturnsAsync((PayoutRequest)null);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.DenyPayout(1, "Reason"));
    }
}
