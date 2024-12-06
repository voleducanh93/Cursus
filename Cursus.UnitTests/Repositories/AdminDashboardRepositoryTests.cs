using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using Cursus.Repository.Repository;
using Cursus.Data.Models;
using Cursus.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class AdminDashboardRepositoryTests
    {
        private AdminDashboardRepository _repository;
        private Mock<CursusDbContext> _dbContextMock;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var dbContext = new CursusDbContext(options);

            // Seed data
            SeedDatabase(dbContext);

            _dbContextMock = new Mock<CursusDbContext>(options) { CallBase = true };
            _repository = new AdminDashboardRepository(dbContext);
        }

        private void SeedDatabase(CursusDbContext context)
        {
            var courses = new List<Course>
            {
                new Course { Id = 1, Name = "Course 1", Description = "Description 1", Price = 100, Rating = 4.5f, DateCreated = new DateTime(2023, 1, 1), Steps = new List<Step> { new Step { Id = 1 } } },
                new Course { Id = 2, Name = "Course 2", Description = "Description 2", Price = 200, Rating = 2.5f, DateCreated = new DateTime(2023, 3, 1), Steps = new List<Step> { new Step { Id = 2 } } }
            };

            context.Courses.AddRange(courses);

            var carts = new List<Cart>
            {
                new Cart { CartId = 1, IsPurchased = true, CartItems = new List<CartItems> { new CartItems { CourseId = 1, Course = courses[0] } } },
                new Cart { CartId = 2, IsPurchased = true, CartItems = new List<CartItems> { new CartItems { CourseId = 2, Course = courses[1] } } }
            };

            context.Cart.AddRange(carts);

            var users = new List<ApplicationUser> { new ApplicationUser { Id = "1" }, new ApplicationUser { Id = "2" } };
            var instructors = new List<InstructorInfo> { new InstructorInfo { Id = 1 } };

            context.Users.AddRange(users);
            context.InstructorInfos.AddRange(instructors);

            context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _dbContextMock.Object.Database.EnsureDeleted();
        }

        [Test]
        public async Task GetTopPurchasedCourses_ReturnsCorrectData()
        {
            var result = await _repository.GetTopPurchasedCourses(2023, "month");

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].CourseName, Is.EqualTo("Course 1"));
        }

        [Test]
        public async Task GetTopPurchasedCourses_WithQuarterPeriod_ReturnsCorrectData()
        {
            var result = await _repository.GetTopPurchasedCourses(2023, "quarter");

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].CourseName, Is.EqualTo("Course 1"));
        }

        [Test]
        public async Task GetWorstRatedCourses_ReturnsCorrectData()
        {
            var result = await _repository.GetWorstRatedCourses(2023, "month");

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].CourseName, Is.EqualTo("Course 2"));
        }

        [Test]
        public async Task GetWorstRatedCourses_WithQuarterPeriod_ReturnsCorrectData()
        {
            var result = await _repository.GetWorstRatedCourses(2023, "quarter");

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].CourseName, Is.EqualTo("Course 2"));
        }

        [Test]
        public async Task GetTotalUsersAsync_ReturnsCorrectCount()
        {
            var result = await _repository.GetTotalUsersAsync();

            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public async Task GetTotalInstructorsAsync_ReturnsCorrectCount()
        {
            var result = await _repository.GetTotalInstructorsAsync();

            Assert.That(result, Is.EqualTo(1));
        }
    }

}
