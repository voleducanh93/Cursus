using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class AdminDashboardServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IMapper> _mapperMock;
        private AdminDashboardService _adminDashboardService;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _adminDashboardService = new AdminDashboardService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Test]
        public async Task GetTopPurchasedCourses_ShouldReturnMappedDtos()
        {
            // Arrange
            var year = 2024;
            var period = "Q1";

            var topCourses = new List<PurchaseCourseOverviewDTO>
    {
        new PurchaseCourseOverviewDTO {  }
    };

            _unitOfWorkMock.Setup(u => u.AdminDashboardRepository.GetTopPurchasedCourses(year, period))
                .ReturnsAsync(topCourses);

            _mapperMock.Setup(m => m.Map<List<PurchaseCourseOverviewDTO>>(topCourses))
                .Returns(topCourses);  

            var result = await _adminDashboardService.GetTopPurchasedCourses(year, period);


            Assert.That(topCourses, Is.EqualTo(result));  
        }


        [Test]
        public async Task GetWorstRatedCourses_ShouldReturnMappedDtos()
        {
            // Arrange
            var year = 2024;
            var period = "Q1";

            // Dữ liệu mô phỏng trả về từ phương thức GetWorstRatedCourses của repository
            var worstCourses = new List<PurchaseCourseOverviewDTO>
    {
        new PurchaseCourseOverviewDTO { /* Thêm dữ liệu mẫu cho DTO */ }
    };

            // Mock IUnitOfWork và AdminDashboardRepository trả về danh sách worstCourses
            _unitOfWorkMock.Setup(u => u.AdminDashboardRepository.GetWorstRatedCourses(year, period))
                .ReturnsAsync(worstCourses); // Trả về Task<List<PurchaseCourseOverviewDTO>>

            // Mock AutoMapper để ánh xạ từ worstCourses sang worstCoursesDto
            _mapperMock.Setup(m => m.Map<List<PurchaseCourseOverviewDTO>>(worstCourses))
                .Returns(worstCourses);  // Chỉ ra rằng AutoMapper sẽ trả về worstCourses (mặc dù worstCourses đã là DTO rồi)

            // Act
            var result = await _adminDashboardService.GetWorstRatedCourses(year, period);

            // Assert
            Assert.That(worstCourses, Is.EqualTo(result));  // Kiểm tra xem kết quả trả về có giống như mong đợi không
        }


        [Test]
        public async Task GetTotalUsersAsync_ShouldReturnUserCount()
        {
            // Arrange
            var totalUsers = 100;

            _unitOfWorkMock.Setup(u => u.AdminDashboardRepository.GetTotalUsersAsync())
                .ReturnsAsync(totalUsers);

            // Act
            var result = await _adminDashboardService.GetTotalUsersAsync();

            // Assert
            Assert.That(totalUsers, Is.EqualTo(result));
        }

        [Test]
        public async Task GetTotalInstructorsAsync_ShouldReturnInstructorCount()
        {
            // Arrange
            var totalInstructors = 50;

            _unitOfWorkMock.Setup(u => u.AdminDashboardRepository.GetTotalInstructorsAsync())
                .ReturnsAsync(totalInstructors);

            // Act
            var result = await _adminDashboardService.GetTotalInstructorsAsync();

            // Assert
            Assert.That(totalInstructors,Is.EqualTo(result));
        }
    }
}
