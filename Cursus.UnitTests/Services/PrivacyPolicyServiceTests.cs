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
using System.Linq.Expressions;
namespace Cursus.UnitTests.Services;
[TestFixture]
public class PrivacyPolicyServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<IMapper> _mapperMock;
    private PrivacyPolicyService _service;

    [SetUp]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _service = new PrivacyPolicyService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task GetPrivacyPolicyAsync_ReturnsPolicies()
    {
        // Arrange
        var policies = new List<PrivacyPolicy>
        {
            new PrivacyPolicy { Id = 1, Content = "Policy1" },
            new PrivacyPolicy { Id = 2, Content = "Policy2" }
        };
        _unitOfWorkMock.Setup(u => u.PrivacyPolicyRepository.GetAllAsync(null,null)).ReturnsAsync(policies);

        // Act
        var result = await _service.GetPrivacyPolicyAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(),Is.EqualTo(2));
        Assert.That(result.First().Content, Is.EqualTo("Policy1"));
    }

    [Test]
    public void GetPrivacyPolicyAsync_ThrowsKeyNotFoundException_WhenNoPolicies()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.PrivacyPolicyRepository.GetAllAsync(null,null)).ReturnsAsync((IEnumerable<PrivacyPolicy>)null);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.GetPrivacyPolicyAsync());
    }

    [Test]
    public async Task CreatePrivacyPolicyAsync_CreatesPolicy()
    {
        // Arrange
        var dto = new PrivacyPolicyDTO { Content = "New Policy" };
        var policy = new PrivacyPolicy { Id = 1, Content = "New Policy" };
        _unitOfWorkMock.Setup(a => a.PrivacyPolicyRepository.AddAsync(It.IsAny<PrivacyPolicy>())).ReturnsAsync(policy);
        _mapperMock.Setup(m => m.Map<PrivacyPolicy>(dto)).Returns(policy);
        _mapperMock.Setup(m => m.Map<PrivacyPolicyDTO>(policy)).Returns(dto);

        // Act
        var result = await _service.CreatePrivacyPolicyAsync(dto);

        // Assert
        Assert.That(result,Is.Not.Null);
        Assert.That(result.Content,Is.EqualTo("New Policy"));
        _unitOfWorkMock.Verify(u => u.PrivacyPolicyRepository.AddAsync(policy), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public async Task DeletePrivacyPolicyAsync_DeletesPolicy()
    {
        // Arrange
        var policy = new PrivacyPolicy { Id = 1, Content = "Policy to delete" };
        _unitOfWorkMock.Setup(u => u.PrivacyPolicyRepository.GetAsync(It.IsAny<Expression<Func<PrivacyPolicy, bool>>>(),null))
            .ReturnsAsync(policy);

        // Act
        await _service.DeletePrivacyPolicyAsync(1);

        // Assert
        _unitOfWorkMock.Verify(u => u.PrivacyPolicyRepository.DeleteAsync(policy), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public void DeletePrivacyPolicyAsync_ThrowsKeyNotFoundException_WhenPolicyNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.PrivacyPolicyRepository.GetAsync(It.IsAny<Expression<Func<PrivacyPolicy, bool>>>(), null))
            .ReturnsAsync((PrivacyPolicy)null);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.DeletePrivacyPolicyAsync(1));
    }

    [Test]
    public async Task UpdatePrivacyPolicyAsync_UpdatesPolicy()
    {
        // Arrange
        var dto = new PrivacyPolicyDTO { Content = "Updated Policy" };
        var policy = new PrivacyPolicy { Id = 1, Content = "Old Policy" };

        _unitOfWorkMock.Setup(u => u.PrivacyPolicyRepository.GetAsync(It.IsAny<Expression<Func<PrivacyPolicy, bool>>>(),null))
            .ReturnsAsync(policy);

        _mapperMock.Setup(m => m.Map(dto, policy)).Callback(() => policy.Content = dto.Content);
        _mapperMock.Setup(m => m.Map<PrivacyPolicyDTO>(policy)).Returns(dto);

        // Act
        var result = await _service.UpdatePrivacyPolicyAsync(1, dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Content, Is.EqualTo("Updated Policy"));
        _unitOfWorkMock.Verify(u => u.PrivacyPolicyRepository.UpdateAsync(policy), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public void UpdatePrivacyPolicyAsync_ThrowsKeyNotFoundException_WhenPolicyNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.PrivacyPolicyRepository.GetAsync(It.IsAny<Expression<Func<PrivacyPolicy, bool>>>(), null))
            .ReturnsAsync((PrivacyPolicy)null);

        var dto = new PrivacyPolicyDTO { Content = "Updated Policy" };

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.UpdatePrivacyPolicyAsync(1, dto));
    }
}
