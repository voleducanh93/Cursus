using NUnit.Framework;
using Moq;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Cursus.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cursus.ServiceContract.Interfaces;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class CourseProgressServiceTests
    {
        private Mock<IProgressRepository> _mockProgressRepository;
        private Mock<ICourseProgressRepository> _mockCourseProgressRepository;
        private CourseProgressService _courseProgressService;

        [SetUp]
        public void SetUp()
        {
            _mockProgressRepository = new Mock<IProgressRepository>();
            _mockCourseProgressRepository = new Mock<ICourseProgressRepository>();
            _courseProgressService = new CourseProgressService(_mockProgressRepository.Object, _mockCourseProgressRepository.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _mockProgressRepository = null;
            _mockCourseProgressRepository = null;
            _courseProgressService = null;
        }

        #region GetRegisteredCourseIdsAsync Tests

        [Test]
        public async Task GetRegisteredCourseIdsAsync_WhenUserIdIsValid_ReturnsCourseIds()
        {
            // Arrange
            var userId = "user1";
            var courseProgresses = new List<CourseProgress>
            {
                new CourseProgress { UserId = userId, CourseId = 1 },
                new CourseProgress { UserId = userId, CourseId = 2 }
            };
            _mockProgressRepository.Setup(repo => repo.GetAllAsync(null, null)).ReturnsAsync(courseProgresses);

            // Act
            var result = await _courseProgressService.GetRegisteredCourseIdsAsync(userId);

            // Assert
            //Assert.AreEqual(2, result.Count());
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.ToList(), Does.Contain(1));
            Assert.That(result.ToList(), Does.Contain(2));
        }

        [Test]
        public async Task GetRegisteredCourseIdsAsync_WhenUserIdIsNull_ReturnsEmptyList()
        {
            // Arrange
            string userId = null;

            // Act
            var result = await _courseProgressService.GetRegisteredCourseIdsAsync(userId);

            // Assert
            Assert.That(result,Is.Empty);
        }

        [Test]
        public async Task GetRegisteredCourseIdsAsync_WhenUserIdIsEmpty_ReturnsEmptyList()
        {
            // Arrange
            var userId = string.Empty;

            // Act
            var result = await _courseProgressService.GetRegisteredCourseIdsAsync(userId);

            // Assert
            Assert.That(result,Is.Empty);
        }

        #endregion

        #region TrackingProgressAsync Tests

        [Test]
        public async Task TrackingProgressAsync_WhenUserIdAndCourseIdAreValid_ReturnsProgressPercentage()
        {
            // Arrange
            var userId = "user1";
            var courseId = 1;
            _mockCourseProgressRepository.Setup(repo => repo.TrackingProgress(userId, courseId)).ReturnsAsync((10, 5));

            // Act
            var result = await _courseProgressService.TrackingProgressAsync(userId, courseId);

            // Assert
            Assert.That(result, Is.EqualTo(50));
        }


        #endregion

        #region Performance Tests

        [Test, Timeout(1000)]
        public async Task TrackingProgressAsync_PerformanceTest()
        {
            // Arrange
            var userId = "user1";
            var courseId = 1;
            _mockCourseProgressRepository.Setup(repo => repo.TrackingProgress(userId, courseId)).ReturnsAsync((1000, 500));

            // Act
            var result = await _courseProgressService.TrackingProgressAsync(userId, courseId);

            // Assert
            Assert.That(result, Is.EqualTo(50));
        }

        #endregion

        #region Concurrency Tests

        [Test]
        public void TrackingProgressAsync_ConcurrencyTest()
        {
            // Arrange
            var userId = "user1";
            var courseId = 1;
            _mockCourseProgressRepository.Setup(repo => repo.TrackingProgress(userId, courseId)).ReturnsAsync((10, 5));

            // Act
            var tasks = new List<Task<double>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_courseProgressService.TrackingProgressAsync(userId, courseId));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert
            foreach (var task in tasks)
            {
                Assert.That(task.Result, Is.EqualTo(50));
            }
        }

        #endregion
    }
}
