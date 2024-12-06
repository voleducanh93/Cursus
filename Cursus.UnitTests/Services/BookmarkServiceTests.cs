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
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
namespace Cursus.UnitTests.Services;

[TestFixture]
public class BookmarkServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<IMapper> _mapperMock;
    private BookmarkService _service;

    [SetUp]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _service = new BookmarkService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task GetFilteredAndSortedBookmarksAsync_ReturnsMappedBookmarks()
    {
        // Arrange
        var bookmarks = new List<Bookmark>
        {
            new Bookmark { Id = 1, UserId = "user1" },
            new Bookmark { Id = 2, UserId = "user1" }
        };

        var bookmarkDTOs = new List<BookmarkDTO>
        {
            new BookmarkDTO { Id = 1 },
            new BookmarkDTO { Id = 2 }
        };

        _unitOfWorkMock.Setup(u => u.BookmarkRepository.GetFilteredAndSortedBookmarksAsync("user1", "date", "asc"))
            .ReturnsAsync(bookmarks);

        _mapperMock.Setup(m => m.Map<IEnumerable<BookmarkDTO>>(bookmarks)).Returns(bookmarkDTOs);

        // Act
        var result = await _service.GetFilteredAndSortedBookmarksAsync("user1", "date", "asc");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result, Is.EqualTo(bookmarkDTOs));
    }

    [Test]
    public async Task GetCourseDetailsAsync_ReturnsMappedCourse()
    {
        // Arrange
        var course = new Course { Id = 1, Name = "Course 1" };
        var courseDTO = new CourseDTO { Id = 1, Name = "Course 1" };

        _unitOfWorkMock.Setup(u => u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(), "Steps"))
            .ReturnsAsync(course);

        _mapperMock.Setup(m => m.Map<CourseDTO>(course)).Returns(courseDTO);

        // Act
        var result = await _service.GetCourseDetailsAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("Course 1"));
    }

    [Test]
    public void GetCourseDetailsAsync_WhenCourseNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(), "Steps"))
            .ReturnsAsync((Course)null);

        // Act & Assert
        Assert.That(async () => await _service.GetCourseDetailsAsync(1), Is.EqualTo(null));
    }

    [Test]
    public async Task CreateBookmarkAsync_AddsNewBookmark()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user1" };
        var course = new Course { Id = 1, Name = "Course 1" };

        _unitOfWorkMock.Setup(u => u.UserRepository.ExiProfile("user1"))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(u => u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(),null))
            .ReturnsAsync(course);

        _unitOfWorkMock.Setup(u => u.BookmarkRepository.GetAsync(It.IsAny<Expression<Func<Bookmark, bool>>>(),null))
            .ReturnsAsync((Bookmark)null);

        // Act
        await _service.CreateBookmarkAsync("user1", 1);

        // Assert
        _unitOfWorkMock.Verify(u => u.BookmarkRepository.AddAsync(It.IsAny<Bookmark>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public void CreateBookmarkAsync_ThrowsKeyNotFoundException_WhenUserNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.UserRepository.ExiProfile("user1"))
            .ReturnsAsync((ApplicationUser)null);

        // Act & Assert
        Assert.That(async () => await _service.CreateBookmarkAsync("user1", 1), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void CreateBookmarkAsync_ThrowsKeyNotFoundException_WhenCourseNotFound()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user1" };

        _unitOfWorkMock.Setup(u => u.UserRepository.ExiProfile("user1"))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(u => u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(), null))
            .ReturnsAsync((Course)null);

        // Act & Assert
        Assert.That(async () => await _service.CreateBookmarkAsync("user1", 1), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void CreateBookmarkAsync_ThrowsBadHttpRequestException_WhenBookmarkAlreadyExists()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user1" };
        var course = new Course { Id = 1, Name = "Course 1" };
        var existingBookmark = new Bookmark { UserId = "user1", Course = course };

        _unitOfWorkMock.Setup(u => u.UserRepository.ExiProfile("user1"))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(u => u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(), null))
            .ReturnsAsync(course);

        _unitOfWorkMock.Setup(u => u.BookmarkRepository.GetAsync(It.IsAny<Expression<Func<Bookmark, bool>>>(), null))
            .ReturnsAsync(existingBookmark);

        // Act & Assert
        Assert.That(async () => await _service.CreateBookmarkAsync("user1", 1), Throws.TypeOf<BadHttpRequestException>());
    }
}
