using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using AutoMapper;
using Cursus.Data.Entities;
using Cursus.Repository.Repository;
using Cursus.Service;
using Cursus.Data.DTO;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
namespace Cursus.UnitTests.Services;

[TestFixture]
public class WalletServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<IMapper> _mapperMock;
    private WalletService _service;

    [SetUp]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _service = new WalletService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task AddMoneyToInstructorWallet_IncreasesWalletBalance()
    {
        // Arrange
        var wallet = new Wallet { UserId = "user1", Balance = 100 };
        _unitOfWorkMock.Setup(u => u.WalletRepository.GetAsync(It.IsAny<Expression<Func<Wallet, bool>>>(),null)).ReturnsAsync(wallet);

        // Act
        await _service.AddMoneyToInstructorWallet("user1", 50);

        // Assert
        Assert.That(wallet.Balance, Is.EqualTo(150));
        _unitOfWorkMock.Verify(u => u.WalletRepository.UpdateAsync(wallet), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public void AddMoneyToInstructorWallet_ThrowsException_WhenWalletNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.WalletRepository.GetAsync(It.IsAny<Expression<Func<Wallet, bool>>>(),null)).ReturnsAsync((Wallet)null);

        // Act & Assert
        Assert.That(async () => await _service.AddMoneyToInstructorWallet("user1", 50), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public async Task CreatePayout_DeductsAmountFromWalletAndCreatesTransaction()
    {
        // Arrange
        var wallet = new Wallet { UserId = "user1", Balance = 100 };
        var instructor = new InstructorInfo { Id = 1, UserId = "user1" };
        var payout = new PayoutRequest { Id = 1,InstructorId = 1 };

        _unitOfWorkMock.Setup(u => u.WalletRepository.GetAsync(It.IsAny<Expression<Func<Wallet, bool>>>(),null)).ReturnsAsync(wallet);
        _unitOfWorkMock.Setup(u => u.InstructorInfoRepository.GetAsync(It.IsAny<Expression<Func<InstructorInfo, bool>>>(), null)).ReturnsAsync(instructor);
        _unitOfWorkMock.Setup(u => u.TransactionRepository.GetNextTransactionId()).ReturnsAsync(1);
        _unitOfWorkMock.Setup(a => a.PayoutRequestRepository.AddAsync(It.IsAny<PayoutRequest>())).ReturnsAsync(payout);
        // Act
        await _service.CreatePayout("user1", 50);

        // Assert
        Assert.That(wallet.Balance, Is.EqualTo(100));
        _unitOfWorkMock.Verify(u => u.TransactionRepository.AddAsync(It.IsAny<Transaction>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.PayoutRequestRepository.AddAsync(It.IsAny<PayoutRequest>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Exactly(2));
    }

    [Test]
    public void CreatePayout_ThrowsException_WhenWalletNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.TransactionRepository.GetNextTransactionId()).ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.WalletRepository.GetAsync(It.IsAny<Expression<Func<Wallet, bool>>>(),null)).ReturnsAsync((Wallet)null);

        // Act & Assert
        Assert.That(async () => await _service.CreatePayout("user1", 50), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void CreatePayout_ThrowsException_WhenInsufficientFunds()
    {
        // Arrange
        var wallet = new Wallet { UserId = "user1", Balance = 30 };
        _unitOfWorkMock.Setup(u => u.WalletRepository.GetAsync(It.IsAny<Expression<Func<Wallet, bool>>>(),null)).ReturnsAsync(wallet);
        _unitOfWorkMock.Setup(u => u.TransactionRepository.GetNextTransactionId()).ReturnsAsync(1);
        // Act & Assert

        Assert.That(async () => await _service.CreatePayout("user1", 50), Throws.TypeOf<BadHttpRequestException>());
    }

    [Test]
    public async Task CreateWallet_CreatesNewWallet()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user1" };
        _unitOfWorkMock.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(),null)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.WalletRepository.GetAsync(It.IsAny<Expression<Func<Wallet, bool>>>(),null)).ReturnsAsync((Wallet)null);

        var walletDTO = new WalletDTO { UserName = "user1", Balance = 0 };
        _mapperMock.Setup(m => m.Map<WalletDTO>(It.IsAny<Wallet>())).Returns(walletDTO);

        // Act
        var result = await _service.CreateWallet("user1");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserName, Is.EqualTo("user1"));
        Assert.That(result.Balance, Is.EqualTo(0));
        _unitOfWorkMock.Verify(u => u.WalletRepository.AddAsync(It.IsAny<Wallet>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public void CreateWallet_ThrowsException_WhenUserNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null)).ReturnsAsync((ApplicationUser)null);

        // Act & Assert
        Assert.That(async () => await _service.CreateWallet("user1"), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void CreateWallet_ThrowsException_WhenWalletAlreadyExists()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user1" };
        var wallet = new Wallet { UserId = "user1", Balance = 100 };

        _unitOfWorkMock.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.WalletRepository.GetAsync(It.IsAny<Expression<Func<Wallet, bool>>>(),null)).ReturnsAsync(wallet);

        // Act & Assert
        Assert.That(async () => await _service.CreateWallet("user1"), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public async Task GetWalletByUserId_ReturnsWallet()
    {
        // Arrange
        var wallet = new Wallet { UserId = "user1", Balance = 100 };
        var walletDTO = new WalletDTO { UserName = "user1", Balance = 100 };

        _unitOfWorkMock.Setup(u => u.WalletRepository.GetAsync(It.IsAny<Expression<Func<Wallet, bool>>>(),null)).ReturnsAsync(wallet);
        _mapperMock.Setup(m => m.Map<WalletDTO>(wallet)).Returns(walletDTO);

        // Act
        var result = await _service.GetWalletByUserId("user1");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserName, Is.EqualTo("user1"));
        Assert.That(result.Balance, Is.EqualTo(100));
    }

    [Test]
    public void GetWalletByUserId_ThrowsException_WhenWalletNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.WalletRepository.GetAsync(It.IsAny<Expression<Func<Wallet, bool>>>(),null)).ReturnsAsync((Wallet)null);

        // Act & Assert
        Assert.That(async () => await _service.GetWalletByUserId("user1"), Throws.TypeOf<KeyNotFoundException>());
    }
}
