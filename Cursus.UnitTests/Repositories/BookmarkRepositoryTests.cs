using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Cursus.Data.Entities;
using Cursus.Repository.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Cursus.Data.Models;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class BookmarkRepositoryTests
    {
        private BookmarkRepository _repository;
        private DbContextOptions<CursusDbContext> _options;
        private CursusDbContext _context;

        [SetUp]
        public void SetUp()
        {
            _options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: "CursusTestDb")
                .Options;

            _context = new CursusDbContext(_options);

            SeedDatabase();
            _repository = new BookmarkRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SeedDatabase()
        {
            _context.Courses.AddRange(new List<Course>
            {
                new Course { Id = 1, Name = "C# Basics", Price = 100, Rating = 4.5 },
                new Course { Id = 2, Name = "ASP.NET Core", Price = 200, Rating = 4.8 },
                new Course { Id = 3, Name = "EF Core", Price = 150, Rating = 4.2 },
            });

            _context.Bookmarks.AddRange(new List<Bookmark>
            {
                new Bookmark { Id = 1, UserId = "user1", CourseId = 1 },
                new Bookmark { Id = 2, UserId = "user1", CourseId = 2 },
                new Bookmark { Id = 3, UserId = "user2", CourseId = 3 },
            });

            _context.SaveChanges();
        }

        [Test]
        public async Task GetFilteredAndSortedBookmarksAsync_ShouldReturnBookmarksForUser()
        {
            var result = await _repository.GetFilteredAndSortedBookmarksAsync("user1", null, "asc");

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(b => b.UserId == "user1"),Is.True);
        }

        [Test]
        public async Task GetFilteredAndSortedBookmarksAsync_ShouldSortByCourseNameAscending()
        {
            var result = await _repository.GetFilteredAndSortedBookmarksAsync("user1", "coursename", "asc");

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Course.Name, Is.EqualTo("ASP.NET Core"));
        }

        [Test]
        public async Task GetFilteredAndSortedBookmarksAsync_ShouldSortByCourseNameDescending()
        {
            var result = await _repository.GetFilteredAndSortedBookmarksAsync("user1", "coursename", "desc");

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Course.Name, Is.EqualTo("C# Basics"));
        }

        [Test]
        public async Task GetFilteredAndSortedBookmarksAsync_ShouldSortByPriceDescending()
        {
            var result = await _repository.GetFilteredAndSortedBookmarksAsync("user1", "price", "desc");

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Course.Price, Is.EqualTo(200));
        }

        [Test]
        public async Task GetFilteredAndSortedBookmarksAsync_ShouldSortByRatingAscending()
        {
            var result = await _repository.GetFilteredAndSortedBookmarksAsync("user1", "rating", "asc");

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Course.Rating, Is.EqualTo(4.5));
        }

        [Test]
        public async Task GetFilteredAndSortedBookmarksAsync_ShouldDefaultSortById()
        {
            var result = await _repository.GetFilteredAndSortedBookmarksAsync("user1", null, null);

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Id, Is.EqualTo(1));
        }
    }
}
