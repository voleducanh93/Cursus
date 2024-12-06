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
public class TermPolicyServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<IMapper> _mapperMock;
    private TermPolicyService _service;

    [SetUp]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _service = new TermPolicyService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task GetTermPolicyAsync_ReturnsPolicies()
    {
        // Arrange
        var terms = new List<Term>
        {
            new Term { Id = 1, Content = "Policy 1" },
            new Term { Id = 2, Content = "Policy 2" }
        };
        _unitOfWorkMock.Setup(u => u.TermPolicyRepository.GetAllAsync(null,null)).ReturnsAsync(terms);

        // Act
        var result = await _service.GetTermPolicyAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Content, Is.EqualTo("Policy 1"));
    }

    [Test]
    public void GetTermPolicyAsync_ThrowsKeyNotFoundException_WhenNoPolicies()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.TermPolicyRepository.GetAllAsync(null,null)).ReturnsAsync((IEnumerable<Term>)null);

        // Act & Assert
        Assert.That(async () => await _service.GetTermPolicyAsync(), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public async Task CreateTermPolicyAsync_CreatesPolicy()
    {
        // Arrange
        var dto = new TermPolicyDTO { Content = "New Policy" };
        var term = new Term { Id = 1, Content = "New Policy" };

        _mapperMock.Setup(m => m.Map<Term>(dto)).Returns(term);
        _mapperMock.Setup(m => m.Map<TermPolicyDTO>(term)).Returns(dto);
        _unitOfWorkMock.Setup(a => a.TermPolicyRepository.AddAsync(It.IsAny<Term>())).ReturnsAsync(term);
        // Act
        var result = await _service.CreateTermPolicyAsync(dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Content, Is.EqualTo("New Policy"));
        _unitOfWorkMock.Verify(u => u.TermPolicyRepository.AddAsync(term), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public async Task DeleteTermPolicyAsync_DeletesPolicy()
    {
        // Arrange
        var term = new Term { Id = 1, Content = "Policy to delete" };
        _unitOfWorkMock.Setup(u => u.TermPolicyRepository.GetAsync(It.IsAny<Expression<Func<Term, bool>>>(),null))
            .ReturnsAsync(term);

        // Act
        await _service.DeleteTermPolicyAsync(1);

        // Assert
        _unitOfWorkMock.Verify(u => u.TermPolicyRepository.DeleteAsync(term), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public void DeleteTermPolicyAsync_ThrowsKeyNotFoundException_WhenPolicyNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.TermPolicyRepository.GetAsync(It.IsAny<Expression<Func<Term, bool>>>(),null))
            .ReturnsAsync((Term)null);

        // Act & Assert
        Assert.That(async () => await _service.DeleteTermPolicyAsync(1), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public async Task UpdateTermPolicyAsync_UpdatesPolicy()
    {
        // Arrange
        var dto = new TermPolicyDTO { Content = "Updated Policy" };
        var term = new Term { Id = 1, Content = "Old Policy" };

        _unitOfWorkMock.Setup(u => u.TermPolicyRepository.GetAsync(It.IsAny<Expression<Func<Term, bool>>>(), null))
            .ReturnsAsync(term);

        _mapperMock.Setup(m => m.Map(dto, term)).Callback(() => term.Content = dto.Content);
        _mapperMock.Setup(m => m.Map<TermPolicyDTO>(term)).Returns(dto);

        // Act
        var result = await _service.UpdateTermPolicyAsync(1, dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Content, Is.EqualTo("Updated Policy"));
        _unitOfWorkMock.Verify(u => u.TermPolicyRepository.UpdateAsync(term), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public void UpdateTermPolicyAsync_ThrowsKeyNotFoundException_WhenPolicyNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.TermPolicyRepository.GetAsync(It.IsAny<Expression<Func<Term, bool>>>(), null))
            .ReturnsAsync((Term)null);

        var dto = new TermPolicyDTO { Content = "Updated Policy" };

        // Act & Assert
        Assert.That(async () => await _service.UpdateTermPolicyAsync(1, dto), Throws.TypeOf<KeyNotFoundException>());
    }
}
