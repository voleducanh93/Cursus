using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Cursus.Data.Entities;
using Cursus.Repository.Repository;
using Cursus.Service;
using Cursus.Data.DTO;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Cursus.ServiceContract.Interfaces;
using System.Linq.Expressions;
namespace Cursus.UnitTests.Services;

[TestFixture]
public class CourseServiceTests
{
    private Mock<IUserService> _userServiceMock;
    private Mock<ICourseProgressService> _progressServiceMock;
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<IMapper> _mapperMock;
    private CourseService _service;

    [SetUp]
    public void Setup()
    {
        _userServiceMock = new Mock<IUserService>();
        _progressServiceMock = new Mock<ICourseProgressService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

        _service = new CourseService(
            _progressServiceMock.Object,
            _userServiceMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );
    }

    [Test]
    public async Task GetCoursesAsync_ReturnsPaginatedCourses()
    {
        // Arrange
        var courses = new List<Course>
        {
            new Course { Id = 1, Name = "Course 1", Status = true },
            new Course { Id = 2, Name = "Course 2", Status = true }
        };

        var courseDTOs = courses.Select(c => new CourseDTO { Id = c.Id, Name = c.Name }).ToList();

        _unitOfWorkMock.Setup(u => u.CourseRepository.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(), "Category"))
            .ReturnsAsync(courses);

        _mapperMock.Setup(m => m.Map<CourseDTO>(It.IsAny<Course>()))
            .Returns((Course source) => new CourseDTO { Id = source.Id, Name = source.Name });

        // Act
        var result = await _service.GetCoursesAsync(null, null, null, 1, 1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(1));
        Assert.That(result.TotalCount, Is.EqualTo(2));
    }

    [Test]
    public async Task GetRegisteredCoursesByUserIdAsync_ReturnsCourses()
    {
        // Arrange
        _userServiceMock.Setup(u => u.CheckUserExistsAsync("user1")).ReturnsAsync(true);
        _progressServiceMock.Setup(p => p.GetRegisteredCourseIdsAsync("user1")).ReturnsAsync(new List<int> { 1 });

        var courses = new List<Course>
        {
            new Course { Id = 1, Name = "Registered Course", Status = true }
        };

        _unitOfWorkMock.Setup(u => u.CourseRepository.GetAllAsync(It.IsAny<Expression<Func<Course, bool>>>(),null))
            .ReturnsAsync(courses);

        _mapperMock.Setup(m => m.Map<CourseDTO>(It.IsAny<Course>()))
            .Returns((Course source) => new CourseDTO { Id = source.Id, Name = source.Name });

        // Act
        var result = await _service.GetRegisteredCoursesByUserIdAsync("user1");

        // Assert
        Assert.That(result.Items.Count, Is.EqualTo(1));
        Assert.That(result.Items.First().Name, Is.EqualTo("Registered Course"));
    }

    [Test]
    public async Task CreateCourseWithSteps_SavesNewCourse()
    {
        // Arrange
        var courseCreateDTO = new CourseCreateDTO
        {
            Name = "New Course",
            CategoryId = 1,
            InstructorInfoId = 1,
            Steps = new List<StepCreateDTO> { new StepCreateDTO { Name = "Step 1" } }
        };

        var course = new Course { Id = 1, Name = "New Course" };
        var courseDTO = new CourseDTO { Id = 1, Name = "New Course" };

        _unitOfWorkMock.Setup(u => u.InstructorInfoRepository.GetAsync(It.IsAny<Expression<Func<InstructorInfo, bool>>>(),null))
            .ReturnsAsync(new InstructorInfo { Id = 1 });

        _unitOfWorkMock.Setup(u => u.CourseRepository.AnyAsync(It.IsAny<Expression<Func<Course, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(u => u.CategoryRepository.GetAsync(It.IsAny<Expression<Func<Category, bool>>>(),null))
            .ReturnsAsync(new Category { Id = 1, IsParent = false });

        _mapperMock.Setup(m => m.Map<Course>(courseCreateDTO)).Returns(course);
        _mapperMock.Setup(m => m.Map<CourseDTO>(course)).Returns(courseDTO);

        _unitOfWorkMock.Setup(u => u.CourseRepository.GetAllIncludeStepsAsync(1)).ReturnsAsync(course);

        // Act
        var result = await _service.CreateCourseWithSteps(courseCreateDTO);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("New Course"));
        _unitOfWorkMock.Verify(u => u.CourseRepository.AddAsync(course), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public async Task DeleteCourse_MarksCourseAsInactive()
    {
        // Arrange
        var course = new Course { Id = 1, Name = "Course to Delete", Status = true };

        _unitOfWorkMock.Setup(u => u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(), null))
            .ReturnsAsync(course);

        // Act
        var result = await _service.DeleteCourse(1);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(course.Status, Is.False);
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }

    [Test]
    public void DeleteCourse_ThrowsException_WhenCourseNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(), null))
            .ReturnsAsync((Course)null);

        // Act & Assert
        Assert.That(async () => await _service.DeleteCourse(1), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public async Task UpdateCourse_UpdatesExistingCourse()
    {
        // Arrange
        var courseUpdateDTO = new CourseUpdateDTO { Id = 1, Name = "Updated Course" };
        var existingCourse = new Course { Id = 1, Name = "Old Course" };
        var updatedCourseDTO = new CourseDTO { Id = 1, Name = "Updated Course" };

        _unitOfWorkMock.Setup(u => u.CourseRepository.GetAsync(It.IsAny<Expression<Func<Course, bool>>>(),null))
            .ReturnsAsync(existingCourse);

        _unitOfWorkMock.Setup(u => u.CourseRepository.AnyAsync(It.IsAny<Expression<Func<Course, bool>>>()))
            .ReturnsAsync(false);

        _mapperMock.Setup(m => m.Map(courseUpdateDTO, existingCourse));
        _mapperMock.Setup(m => m.Map<CourseDTO>(existingCourse)).Returns(updatedCourseDTO);

        // Act
        var result = await _service.UpdateCourse(courseUpdateDTO);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Updated Course"));
        _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
    }
}
