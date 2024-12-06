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
namespace Cursus.UnitTests.Services;
[TestFixture]
public class WalletHistoryServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<IMapper> _mapperMock;
    private WalletHistoryService _service;

    [SetUp]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _service = new WalletHistoryService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task GetWalletHistoriesAsync_ReturnsPaginatedResult()
    {
        // Arrange
        var walletHistories = new List<WalletHistory>
        {
            new WalletHistory { Id = 1, Description = "History 1", AmountChanged = 50, DateLogged = DateTime.UtcNow },
            new WalletHistory { Id = 2, Description = "History 2", AmountChanged = 100, DateLogged = DateTime.UtcNow }
        };
        _unitOfWorkMock.Setup(u => u.WalletHistoryRepository.GetAllAsync(null,null)).ReturnsAsync(walletHistories);

        var walletHistoryDTOs = new List<WalletHistoryDTO>
        {
            new WalletHistoryDTO { WalletId = 1, Description = "History 1", AmountChanged = 50 },
            new WalletHistoryDTO { WalletId = 2, Description = "History 2", AmountChanged = 100 }
        };
        _mapperMock.Setup(m => m.Map<IEnumerable<WalletHistoryDTO>>(It.IsAny<IEnumerable<WalletHistory>>()))
            .Returns(walletHistoryDTOs);

        // Act
        var result = await _service.GetWalletHistoriessAsync(null, null, null, 1, 2);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(2));
        Assert.That(result.Page, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(2));
    }

    [Test]
    public void GetWalletHistoriesAsync_ThrowsKeyNotFoundException_WhenNoHistoriesFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.WalletHistoryRepository.GetAllAsync(null,null))
            .ReturnsAsync(new List<WalletHistory>());

        // Act & Assert
        Assert.That(async () => await _service.GetWalletHistoriessAsync(null, null, null), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public async Task GetByIdAsync_ReturnsWalletHistory()
    {
        // Arrange
        var walletHistory = new WalletHistory { Id = 1, Description = "History 1" };
        var walletHistoryDTO = new WalletHistoryDTO { WalletId = 1, Description = "History 1" };

        _unitOfWorkMock.Setup(u => u.WalletHistoryRepository.GetByIdAsync(1)).ReturnsAsync(walletHistory);
        _mapperMock.Setup(m => m.Map<WalletHistoryDTO>(walletHistory)).Returns(walletHistoryDTO);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Description, Is.EqualTo("History 1"));
    }

    [Test]
    public void GetByIdAsync_ThrowsKeyNotFoundException_WhenHistoryNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.WalletHistoryRepository.GetByIdAsync(1)).ReturnsAsync((WalletHistory)null);

        // Act & Assert
        Assert.That(async () => await _service.GetByIdAsync(1), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public async Task GetByWalletId_ReturnsWalletHistories()
    {
        // Arrange
        var walletHistories = new List<WalletHistory>
        {
            new WalletHistory { Id = 1, Description = "History 1", WalletId = 1 },
            new WalletHistory { Id = 2, Description = "History 2", WalletId = 1 }
        };
        var walletHistoryDTOs = new List<WalletHistoryDTO>
        {
            new WalletHistoryDTO { WalletId = 1, Description = "History 1" },
            new WalletHistoryDTO { WalletId = 2, Description = "History 2" }
        };

        _unitOfWorkMock.Setup(u => u.WalletHistoryRepository.GetByWalletId(1)).ReturnsAsync(walletHistories);
        _mapperMock.Setup(m => m.Map<IEnumerable<WalletHistoryDTO>>(walletHistories)).Returns(walletHistoryDTOs);

        // Act
        var result = await _service.GetByWalletId(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public void GetByWalletId_ThrowsKeyNotFoundException_WhenNoHistoriesFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.WalletHistoryRepository.GetByWalletId(1)).ReturnsAsync(new List<WalletHistory>());

        // Act & Assert
        Assert.That(async () => await _service.GetByWalletId(1), Throws.TypeOf<KeyNotFoundException>());
    }
}
