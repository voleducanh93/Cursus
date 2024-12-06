using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Cursus.Data.Entities;
using Cursus.Repository.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Moq;
using Cursus.Data.Models;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class CartItemsRepositoryTests
    {
        private CartItemsRepository _repository;
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
            _repository = new CartItemsRepository(_context);
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
                new Course { Id = 1, Name = "Course 1", Price = 100 },
                new Course { Id = 2, Name = "Course 2", Price = 200 }
            });

            _context.Cart.Add(new Cart { CartId = 1, UserId = "user1" });

            _context.CartItems.AddRange(new List<CartItems>
            {
                new CartItems { CartItemsId = 1, CartId = 1, CourseId = 1 },
                new CartItems { CartItemsId = 2, CartId = 1, CourseId = 2 }
            });

            _context.SaveChanges();
        }

        [Test]
        public async Task GetAllItems_ShouldReturnAllItemsForGivenCartId()
        {
            var result = await _repository.GetAllItems(1);

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(ci => ci.CartId == 1), Is.True);
        }

        [Test]
        public async Task GetAllItems_ShouldReturnEmptyListForInvalidCartId()
        {
            var result = await _repository.GetAllItems(999);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetItemByID_ShouldReturnItemWithMatchingId()
        {
            var result = await _repository.GetItemByID(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.CartItemsId,Is.EqualTo(1));
        }

        [Test]
        public async Task GetItemByID_ShouldReturnNullForNonexistentItemId()
        {
            var result = await _repository.GetItemByID(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task DeleteCartItems_ShouldReturnTrueForExistingItem()
        {
            var itemToDelete = _context.CartItems.First();

            var result = await _repository.DeleteCartItems(itemToDelete);

            Assert.That(result, Is.True);
            Assert.That(_context.CartItems.Any(ci => ci.CartItemsId == itemToDelete.CartItemsId),Is.True);
        }

        [Test]
        public async Task DeleteCartItems_ShouldReturnTrueForNonexistentItem()
        {
            var nonexistentItem = new CartItems { CartItemsId = 999, CartId = 1, CourseId = 1 };

            var result = await _repository.DeleteCartItems(nonexistentItem);

            Assert.That(result,Is.True);
        }
    }
}