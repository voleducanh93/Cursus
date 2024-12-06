using AutoMapper;
using Cursus.Data.DTO;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class InstructorDashboardServiceTests
    {
        private InstructorDashboardService _instructorDashboardService;
        private Mock<IInstructorDashboardRepository> _instructorDashboardRepositoryMock;
        private Mock<IMapper> _mapperMock;

        [SetUp]
        public void Setup()
        {
            _instructorDashboardRepositoryMock = new Mock<IInstructorDashboardRepository>();
            _mapperMock = new Mock<IMapper>();
            _instructorDashboardService = new InstructorDashboardService(
                _instructorDashboardRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        [Test]
        public async Task GetInstructorDashboardAsync_ValidInstructorId_ReturnsDashboardDTO()
        {
            // Arrange
            int instructorId = 1;
            var dashboardData = new InstructorDashboardDTO { /* initialize with test data */ };
            _instructorDashboardRepositoryMock
                .Setup(repo => repo.GetInstructorDashboardAsync(instructorId))
                .ReturnsAsync(dashboardData);

            // Act
            var result = await _instructorDashboardService.GetInstructorDashboardAsync(instructorId);

            // Assert
            Assert.That(result, Is.EqualTo(dashboardData));
            _instructorDashboardRepositoryMock.Verify(repo => repo.GetInstructorDashboardAsync(instructorId), Times.Once);
        }

        [Test]
        public async Task GetCourseEarningsAsync_ValidInstructorId_ReturnsMappedCourseEarnings()
        {
            // Arrange
            int instructorId = 1;
            var courseEntities = new List<CourseEarningsDTO>
            {
                new CourseEarningsDTO { Price = 100 },
                new CourseEarningsDTO { Price = 200 }
            };

            var mappedCourseEarnings = new List<CourseEarningsDTO>
            {
                new CourseEarningsDTO { Price = 100, PotentialEarnings = 100 * 0.475 * 12 },
                new CourseEarningsDTO { Price = 200, PotentialEarnings = 200 * 0.475 * 12 }
            };

            _instructorDashboardRepositoryMock
                .Setup(repo => repo.GetCourseEarningsAsync(instructorId))
                .ReturnsAsync(courseEntities);

            _mapperMock
                .Setup(m => m.Map<List<CourseEarningsDTO>>(courseEntities))
                .Returns(mappedCourseEarnings);

            // Act
            var result = await _instructorDashboardService.GetCourseEarningsAsync(instructorId);

            // Assert
            Assert.That(result, Is.EqualTo(mappedCourseEarnings));
            _instructorDashboardRepositoryMock.Verify(repo => repo.GetCourseEarningsAsync(instructorId), Times.Once);
        }
    }
}
