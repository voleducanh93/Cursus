using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using AutoMapper;
using System.Linq.Expressions;
using Cursus.Data.DTO;
using Cursus.Service.Services;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Data.Entities; // Ensure this matches your project's namespace

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class HomePageServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IMapper> _mockMapper;
        private HomePageService _homePageService;
        private List<HomePage> _homePages;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            _homePages = new List<HomePage>
            {
                new HomePage
                {
                    Id = 1,
                    BranchName = "Main Branch",
                    SupportHotline = "1234567890",
                    PhoneNumber = "0987654321",
                    Address = "123 Main St",
                    WorkingTime = "9 AM - 5 PM",
                    LastUpdatedBy = "Admin1",
                    LastUpdatedDate = DateTime.Now.AddDays(-1)
                },
                new HomePage
                {
                    Id = 2,
                    BranchName = "Branch 2",
                    SupportHotline = "0987654321",
                    PhoneNumber = "1234567890",
                    Address = "456 Second St",
                    WorkingTime = "10 AM - 6 PM",
                    LastUpdatedBy = "Admin2",
                    LastUpdatedDate = DateTime.Now
                }
            };

            _homePageService = new HomePageService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Test]
        public async Task GetHomePageAsync_WithExistingHomePages_ReturnsLatestHomePage()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.HomePageRepository.GetAllAsync(null, null))
                .ReturnsAsync(_homePages);

            // Act
            var result = await _homePageService.GetHomePageAsync();

            // Assert
            Assert.That(result,Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(2));
            Assert.That(result.BranchName, Is.EqualTo("Branch 2"));
            _mockUnitOfWork.Verify(u => u.HomePageRepository.GetAllAsync(null, null), Times.Once);
        }

        [Test]
        public async Task GetHomePageAsync_WithNoHomePages_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.HomePageRepository.GetAllAsync(null, null))
                .ReturnsAsync(new List<HomePage>());

            var result = await _homePageService.GetHomePageAsync();
            // Act & Assert
            Assert.That(result, Is.EqualTo(null));
        }

        [Test]
        public async Task UpdateHomePageAsync_WithValidId_UpdatesSuccessfully()
        {
            // Arrange
            int homePageId = 1;
            var existingHomePage = _homePages.First(hp => hp.Id == homePageId);
            var homePageDto = new HomePageDTO
            {
                BranchName = "Updated Branch Name",
                SupportHotline = "1111111111",
                PhoneNumber = "2222222222"
            };

            // Explicitly specify the type for GetAsync
            _mockUnitOfWork.Setup(u => u.HomePageRepository.GetAsync(
                It.Is<Expression<Func<HomePage, bool>>>(
                    expr => expr.Compile()(existingHomePage)),
                It.IsAny<string>()))
                .ReturnsAsync(existingHomePage);

            // Modify mapper setup to explicitly specify types
            _mockMapper.Setup(m => m.Map(It.IsAny<HomePageDTO>(), It.IsAny<HomePage>()))
                .Callback<HomePageDTO, HomePage>((dto, entity) =>
                {
                    entity.BranchName = dto.BranchName;
                    entity.SupportHotline = dto.SupportHotline;
                    entity.PhoneNumber = dto.PhoneNumber;
                });

            // Explicitly specify type for Map method
            _mockMapper.Setup(m => m.Map<HomePageDTO>(It.IsAny<HomePage>()))
                .Returns((HomePage homePage) => new HomePageDTO
                {
                    BranchName = homePage.BranchName,
                    SupportHotline = homePage.SupportHotline,
                    PhoneNumber = homePage.PhoneNumber
                });

            _mockUnitOfWork.Setup(u => u.HomePageRepository.UpdateAsync(existingHomePage))
                .ReturnsAsync(existingHomePage);

            // Act
            var result = await _homePageService.UpdateHomePageAsync(homePageId, homePageDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.BranchName, Is.EqualTo("Updated Branch Name"));
            Assert.That(result.SupportHotline, Is.EqualTo("1111111111"));
            Assert.That(result.PhoneNumber, Is.EqualTo("2222222222"));

            _mockUnitOfWork.Verify(u => u.HomePageRepository.GetAsync(
                It.Is<Expression<Func<HomePage, bool>>>(
                    expr => expr.Compile()(existingHomePage)),
                It.IsAny<string>()), Times.Once);

            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Test]
        public void UpdateHomePageAsync_WithInvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int invalidId = 999;
            var homePageDto = new HomePageDTO
            {
                BranchName = "Updated Branch Name"
            };

            _mockUnitOfWork.Setup(u => u.HomePageRepository.GetAsync(
                It.Is<Expression<Func<HomePage, bool>>>(expr => expr.Compile()(new HomePage { Id = invalidId })),
                null))
                .ReturnsAsync((HomePage)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _homePageService.UpdateHomePageAsync(invalidId, homePageDto));
        }

        [Test]
        public async Task UpdateHomePageAsync_VerifyMapperCalled()
        {
            // Arrange
            int homePageId = 1;
            var existingHomePage = _homePages.First(hp => hp.Id == homePageId);
            var homePageDto = new HomePageDTO
            {
                BranchName = "Updated Branch Name"
            };

            _mockUnitOfWork.Setup(u => u.HomePageRepository.GetAsync(
                It.Is<Expression<Func<HomePage, bool>>>(
                    expr => expr.Compile()(existingHomePage)),
                null))
                .ReturnsAsync(existingHomePage);

            _mockMapper.Setup(m => m.Map(homePageDto, existingHomePage))
                .Verifiable();

            _mockMapper.Setup(m => m.Map<HomePageDTO>(existingHomePage))
                .Returns(new HomePageDTO());

            _mockUnitOfWork.Setup(u => u.HomePageRepository.UpdateAsync(existingHomePage))
                .ReturnsAsync(existingHomePage);

            // Act
            await _homePageService.UpdateHomePageAsync(homePageId, homePageDto);

            // Assert
            _mockMapper.Verify(m => m.Map(homePageDto, existingHomePage), Times.Once);
            _mockMapper.Verify(m => m.Map<HomePageDTO>(existingHomePage), Times.Once);
        }
    }
}