using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Cursus.Data.Entities;
using Cursus.Data.Enums;
using Cursus.Repository.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cursus.Data.Models;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class PayoutRequestRepositoryTests
    {
        private PayoutRequestRepository _repository;
        private CursusDbContext _dbContext;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new CursusDbContext(options);
            SeedDatabase();
            _repository = new PayoutRequestRepository(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        private void SeedDatabase()
        {
            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, Amount = 1000, DateCreated = DateTime.Now },
                new Transaction { TransactionId = 2, Amount = 2000, DateCreated = DateTime.Now }
            };

            var payoutRequests = new List<PayoutRequest>
            {
                new PayoutRequest { Id = 1, InstructorId = 1, TransactionId = 1, CreatedDate = DateTime.Now, PayoutRequestStatus = PayoutRequestStatus.Approved },
                new PayoutRequest { Id = 2, InstructorId = 2, TransactionId = 2, CreatedDate = DateTime.Now, PayoutRequestStatus = PayoutRequestStatus.Pending },
                new PayoutRequest { Id = 3, InstructorId = 3, TransactionId = 1, CreatedDate = DateTime.Now, PayoutRequestStatus = PayoutRequestStatus.Rejected }
            };

            _dbContext.Transactions.AddRange(transactions);
            _dbContext.PayoutRequests.AddRange(payoutRequests);
            _dbContext.SaveChanges();
        }

        [Test]
        public async Task GetApprovedPayoutAsync_ReturnsApprovedPayouts()
        {
            var result = await _repository.GetApprovedPayoutAsync();

            Assert.That( result.Count(),Is.EqualTo(1));
            Assert.That(result.All(x => x.PayoutRequestStatus == PayoutRequestStatus.Approved),Is.True);
        }

        [Test]
        public async Task GetPendingPayoutAsync_ReturnsPendingPayouts()
        {
            var result = await _repository.GetPendingPayoutAsync();

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.All(x => x.PayoutRequestStatus == PayoutRequestStatus.Pending),Is.True);
        }

        [Test]
        public async Task GetRejectedPayoutAsync_ReturnsRejectedPayouts()
        {
            var result = await _repository.GetRejectedPayoutAsync();

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.All(x => x.PayoutRequestStatus == PayoutRequestStatus.Rejected),Is.True);
        }

        [Test]
        public async Task GetPayoutByID_ReturnsCorrectPayout()
        {
            var result = await _repository.GetPayoutByID(1);

            Assert.That(result,Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.PayoutRequestStatus,Is.EqualTo(PayoutRequestStatus.Approved));
        }

        [Test]
        public async Task GetPayoutByID_ReturnsNullForNonExistingId()
        {
            var result = await _repository.GetPayoutByID(99);

            Assert.That(result, Is.Null);
        }
    }
}
