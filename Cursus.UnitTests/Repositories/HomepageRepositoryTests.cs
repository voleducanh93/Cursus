using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.Repository.Repository;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class HomePageRepositoryTests
    {
        private CursusDbContext _context;
        private HomePageRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CursusDbContext(options);
            _repository = new HomePageRepository(_context);
        }

        [TearDown]
        public void Teardown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task AddAsync_ShouldAddHomePageSuccessfully()
        {
            // Arrange
            var homePage = new HomePage
            {
                BranchName = "Test Branch",
                SupportHotline = "1234567890",
                PhoneNumber = "0987654321",
                Address = "Test Address",
                WorkingTime = "9 AM - 5 PM",
                LastUpdatedBy = "TestUser",
                LastUpdatedDate = DateTime.Now
            };

            // Act
            var addedHomePage = await _repository.AddAsync(homePage);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(addedHomePage,Is.Not.Null);
            Assert.That(addedHomePage.BranchName,Is.EqualTo(homePage.BranchName));
            Assert.That(_context.Set<HomePage>().Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateHomePageSuccessfully()
        {
            // Arrange
            var homePage = new HomePage
            {
                BranchName = "Original Branch",
                SupportHotline = "1234567890",
                PhoneNumber = "0987654321",
                Address = "Original Address",
                WorkingTime = "9 AM - 5 PM",
                LastUpdatedBy = "OriginalUser",
                LastUpdatedDate = DateTime.Now
            };

            await _context.Set<HomePage>().AddAsync(homePage);
            await _context.SaveChangesAsync();

            // Act
            homePage.BranchName = "Updated Branch";
            homePage.LastUpdatedBy = "UpdatedUser";
            var updatedHomePage = await _repository.UpdateAsync(homePage);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(updatedHomePage, Is.Not.Null);
            Assert.That(updatedHomePage.BranchName, Is.EqualTo("Updated Branch"));
            Assert.That(updatedHomePage.LastUpdatedBy, Is.EqualTo("UpdatedUser"));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveHomePageSuccessfully()
        {
            // Arrange
            var homePage = new HomePage
            {
                BranchName = "Branch to Delete",
                SupportHotline = "1234567890",
                PhoneNumber = "0987654321",
                Address = "Delete Address",
                WorkingTime = "9 AM - 5 PM",
                LastUpdatedBy = "DeleteUser",
                LastUpdatedDate = DateTime.Now
            };

            await _context.Set<HomePage>().AddAsync(homePage);
            await _context.SaveChangesAsync();

            // Act
            var deletedHomePage = await _repository.DeleteAsync(homePage);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(deletedHomePage, Is.Not.Null);
            Assert.That(_context.Set<HomePage>().Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetAsync_WithFilter_ShouldReturnCorrectHomePage()
        {
            // Arrange
            var homePages = new[]
            {
                new HomePage
                {
                    BranchName = "Branch1",
                    SupportHotline = "1111111111",
                    PhoneNumber = "2222222222",
                    Address = "Address1",
                    WorkingTime = "9 AM - 5 PM",
                    LastUpdatedBy = "User1",
                    LastUpdatedDate = DateTime.Now
                },
                new HomePage
                {
                    BranchName = "Branch2",
                    SupportHotline = "3333333333",
                    PhoneNumber = "4444444444",
                    Address = "Address2",
                    WorkingTime = "10 AM - 6 PM",
                    LastUpdatedBy = "User2",
                    LastUpdatedDate = DateTime.Now.AddDays(1)
                }
            };

            await _context.Set<HomePage>().AddRangeAsync(homePages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAsync(h => h.BranchName == "Branch1");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.BranchName, Is.EqualTo("Branch1"));
            Assert.That(result.SupportHotline, Is.EqualTo("1111111111"));
        }

        [Test]
        public async Task GetAllAsync_WithoutFilter_ShouldReturnAllHomePages()
        {
            // Arrange
            var homePages = new[]
            {
                new HomePage
                {
                    BranchName = "Branch1",
                    SupportHotline = "1111111111",
                    PhoneNumber = "2222222222",
                    Address = "Address1",
                    WorkingTime = "9 AM - 5 PM",
                    LastUpdatedBy = "User1",
                    LastUpdatedDate = DateTime.Now
                },
                new HomePage
                {
                    BranchName = "Branch2",
                    SupportHotline = "3333333333",
                    PhoneNumber = "4444444444",
                    Address = "Address2",
                    WorkingTime = "10 AM - 6 PM",
                    LastUpdatedBy = "User2",
                    LastUpdatedDate = DateTime.Now.AddDays(1)
                }
            };

            await _context.Set<HomePage>().AddRangeAsync(homePages);
            await _context.SaveChangesAsync();

            // Act
            var results = await _repository.GetAllAsync();

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetAsync_NonExistentRecord_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetAsync(h => h.BranchName == "NonExistentBranch");

            // Assert
            Assert.That(result, Is.Null);
        }
    }
}