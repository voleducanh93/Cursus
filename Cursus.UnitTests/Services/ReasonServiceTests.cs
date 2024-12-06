using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Cursus.Data.Entities;
using Cursus.Data.Enums;
using Cursus.Repository.Repository;
using Cursus.Service;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Cursus.ServiceContract.Interfaces;
using Cursus.Data.DTO;
using Cursus.Repository.Enum;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
namespace Cursus.UnitTests.Services;
[TestFixture]
public class ReasonServiceTests
{
    private Mock<IReasonRepository> _reasonRepositoryMock;
    private Mock<ICourseService> _courseServiceMock;
    private Mock<IMapper> _mapperMock;
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private ReasonService _service;

    [SetUp]
    public void Setup()
    {
        _reasonRepositoryMock = new Mock<IReasonRepository>();
        _courseServiceMock = new Mock<ICourseService>();
        _mapperMock = new Mock<IMapper>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new ReasonService(
            _reasonRepositoryMock.Object,
            _mapperMock.Object,
            _unitOfWorkMock.Object,
            _courseServiceMock.Object
        );
    }

    [Test]
    public async Task CreateReason_CreatesReasonSuccessfully()
    {
        // Arrange
        var dto = new CreateReasonDTO { Description = "Test Reason", CourseId = 1 };
        var course = new CourseDTO { Id = 1 };
        var reason = new Reason { Id = 1, Description = "Test Reason", CourseId = 1, Status = (int)ReasonStatus.Processing };

        _courseServiceMock.Setup(c => c.GetCourseByIdAsync(1)).ReturnsAsync(course);
        _mapperMock.Setup(m => m.Map<Reason>(dto)).Returns(reason);

        // Act
        var result = await _service.CreateReason(dto);

        // Assert
        Assert.That(result,Is.Not.Null);
        Assert.That( result.Status,Is.EqualTo((int)ReasonStatus.Processing));
        _reasonRepositoryMock.Verify(r => r.AddAsync(reason), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public void CreateReason_ThrowsBadHttpRequestException_WhenDescriptionIsMissing()
    {
        // Arrange
        var dto = new CreateReasonDTO { Description = null, CourseId = 1 };

        // Act & Assert
        Assert.ThrowsAsync<BadHttpRequestException>(async () => await _service.CreateReason(dto));
    }

    [Test]
    public void CreateReason_ThrowsBadHttpRequestException_WhenCourseDoesNotExist()
    {
        // Arrange
        var dto = new CreateReasonDTO { Description = "Test Reason", CourseId = 1 };
        _courseServiceMock.Setup(c => c.GetCourseByIdAsync(1)).ReturnsAsync((CourseDTO)null);

        // Act & Assert
        Assert.ThrowsAsync<BadHttpRequestException>(async () => await _service.CreateReason(dto));
    }

    [Test]
    public async Task GetReasonByIdAsync_ReturnsReason()
    {
        // Arrange
        var reason = new Reason { Id = 1, Description = "Test Reason" };
        var dto = new ReasonDTO { Id = 1, Description = "Test Reason" };

        _reasonRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reason);
        _mapperMock.Setup(m => m.Map<ReasonDTO>(reason)).Returns(dto);

        // Act
        var result = await _service.GetReasonByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Description, Is.EqualTo("Test Reason"));
    }

    [Test]
    public void GetReasonByIdAsync_ThrowsKeyNotFoundException_WhenReasonNotFound()
    {
        // Arrange
        _reasonRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Reason)null);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.GetReasonByIdAsync(1));
    }

    [Test]
    public async Task DeleteReasonAsync_DeletesReason()
    {
        // Arrange
        var reason = new Reason { Id = 1, Description = "Test Reason" };
        _reasonRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reason);
        _unitOfWorkMock.Setup(a => a.ReasonRepository.DeleteAsync(It.IsAny<Reason>()));

        // Act
        await _service.DeleteReasonAsync(1);

        // Assert
        _unitOfWorkMock.Verify(u => u.ReasonRepository.DeleteAsync(reason), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public void DeleteReasonAsync_ThrowsKeyNotFoundException_WhenReasonNotFound()
    {
        // Arrange
        _reasonRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Reason)null);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.DeleteReasonAsync(1));
    }

    [Test]
    public async Task GetByCourseIdAsync_ReturnsReasons()
    {
        // Arrange
        var reasons = new List<Reason>
        {
            new Reason { Id = 1, Description = "Reason1", CourseId = 1 },
            new Reason { Id = 2, Description = "Reason2", CourseId = 1 }
        };

        _reasonRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Reason, bool>>>(),null)).ReturnsAsync(reasons);

        // Act
        var result = await _service.GetByCourseIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public void GetByCourseIdAsync_ThrowsBadHttpRequestException_WhenNoReasonsFound()
    {
        // Arrange
        _reasonRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Reason, bool>>>(),null)).ReturnsAsync(new List<Reason>());

        // Act & Assert
        Assert.ThrowsAsync<BadHttpRequestException>(async () => await _service.GetByCourseIdAsync(1));
    }
}
