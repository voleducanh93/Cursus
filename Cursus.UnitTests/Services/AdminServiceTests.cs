using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cursus.Data.Entities;
using Cursus.Repository.Repository;
using Cursus.Service;
using Microsoft.AspNetCore.Identity;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Cursus.ServiceContract.Interfaces;
using Cursus.Data.Enums;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
namespace Cursus.UnitTests.Services;
[TestFixture]
public class AdminServiceTests
{
    private Mock<IAdminRepository> _adminRepositoryMock;
    private Mock<IInstructorInfoRepository> _instructorInfoRepositoryMock;
    private Mock<UserManager<ApplicationUser>> _userManagerMock;
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private AdminService _service;

    [SetUp]
    public void Setup()
    {
        _adminRepositoryMock = new Mock<IAdminRepository>();
        _instructorInfoRepositoryMock = new Mock<IInstructorInfoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userManagerMock = MockUserManager();

        _service = new AdminService(
            _adminRepositoryMock.Object,
            _instructorInfoRepositoryMock.Object,
            _userManagerMock.Object,
            _unitOfWorkMock.Object
        );
    }

    private Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
    }

    [Test]
    public async Task AcceptPayout_UpdatesTransactionStatusAndDeductsWalletBalance()
    {
        // Arrange
        var transaction = new Transaction
        {
            TransactionId = 1,
            UserId = "user1",
            Description = "payout",
            Amount = 100,
            Status = TransactionStatus.Pending
        };
        var wallet = new Wallet { UserId = "user1", Balance = 200 };

        _unitOfWorkMock.Setup(u => u.TransactionRepository.GetAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), null))
            .ReturnsAsync(transaction);
        _unitOfWorkMock.Setup(u => u.WalletRepository.GetAsync(It.IsAny<Expression<Func<Wallet, bool>>>(), null))
            .ReturnsAsync(wallet);

        // Act
        var result = await _service.AcceptPayout(1);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(transaction.Status, Is.EqualTo(TransactionStatus.Completed));
        Assert.That(wallet.Balance, Is.EqualTo(100));
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public void AcceptPayout_ThrowsException_WhenTransactionNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.TransactionRepository.GetAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), null))
            .ReturnsAsync((Transaction)null);

        // Act & Assert
        Assert.That(async () => await _service.AcceptPayout(1), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void AcceptPayout_ThrowsException_WhenDescriptionIsInvalid()
    {
        // Arrange
        var transaction = new Transaction
        {
            TransactionId = 1,
            Description = "invalid"
        };

        _unitOfWorkMock.Setup(u => u.TransactionRepository.GetAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), null))
            .ReturnsAsync(transaction);

        // Act & Assert
        Assert.That(async () => await _service.AcceptPayout(1), Throws.TypeOf<BadHttpRequestException>());
    }

    [Test]
    public async Task AdminComments_SavesAdminComment()
    {
        // Arrange
        _adminRepositoryMock.Setup(a => a.AdminComments("user1", "Great instructor"))
            .ReturnsAsync(true);

        // Act
        var result = await _service.AdminComments("user1", "Great instructor");

        // Assert
        Assert.That(result, Is.True);
        _adminRepositoryMock.Verify(a => a.AdminComments("user1", "Great instructor"), Times.Once);
    }

    [Test]
    public async Task GetAllUser_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user1" },
            new ApplicationUser { Id = "user2" }
        };

        _adminRepositoryMock.Setup(a => a.GetAllAsync(null,null))
            .ReturnsAsync(users);

        // Act
        var result = await _service.GetAllUser();

        // Assert
        Assert.That(result, Is.Not.Null);
       Assert.That(result.Count(),Is.EqualTo(2));
    }

    [Test]
    public async Task GetInformationInstructor_ReturnsInstructorDetails()
    {
        // Arrange
        var instructorInfo = new InstructorInfo { Id = 1, UserId = "user1" };
        var wallet = new Wallet { UserId = "user1", Balance = 500 };

        _unitOfWorkMock.Setup(u => u.InstructorInfoRepository.GetAsync(It.IsAny<Expression<Func<InstructorInfo, bool>>>(),null))
            .ReturnsAsync(instructorInfo);

        _unitOfWorkMock.Setup(u => u.WalletRepository.GetAsync(It.IsAny<Expression<Func<Wallet, bool>>>(),null))
            .ReturnsAsync(wallet);

        _adminRepositoryMock.Setup(a => a.GetInformationInstructorAsync(1))
            .ReturnsAsync((
            
                "Instructor1",
                "instructor1@example.com",
                "123456789",
                "Excellent",
                100
            ));

        _instructorInfoRepositoryMock.Setup(i => i.TotalCourse(1)).ReturnsAsync(5);
        _instructorInfoRepositoryMock.Setup(i => i.TotalActiveCourse(1)).ReturnsAsync(3);
        _instructorInfoRepositoryMock.Setup(i => i.TotalPayout(1)).ReturnsAsync(1000);
        _instructorInfoRepositoryMock.Setup(i => i.RatingNumber(1)).ReturnsAsync(4.5);

        // Act
        var result = await _service.GetInformationInstructor(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result["UserName"], Is.EqualTo("Instructor1"));
        Assert.That(result["TotalEarning"], Is.EqualTo(100));
    }

    [Test]
    public async Task ToggleUserStatusAsync_TogglesStatusSuccessfully()
    {
        // Arrange
        _adminRepositoryMock.Setup(a => a.ToggleUserStatusAsync("user1")).ReturnsAsync(true);

        // Act
        var result = await _service.ToggleUserStatusAsync("user1");

        // Assert
        Assert.That(result, Is.True);
        _adminRepositoryMock.Verify(a => a.ToggleUserStatusAsync("user1"), Times.Once);
    }
}
