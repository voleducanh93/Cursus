using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Moq;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.Repository.Repository;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class InstructorCertificateRepositoryTests
    {
        private CursusDbContext _context;
        private InstructorCertificateRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CursusDbContext(options);
            _repository = new InstructorCertificateRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task GetInstructorIdByUserIdAsync_ExistingUser_ReturnsInstructorId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var instructorInfo = new Cursus.Data.Entities.InstructorInfo
            {
                Id = 1,
                UserId = userId.ToString()
            };

            _context.InstructorInfos.Add(instructorInfo);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetInstructorIdByUserIdAsync(userId);

            // Assert
            Assert.That(result,Is.EqualTo(1));
        }

        [Test]
        public async Task GetInstructorIdByUserIdAsync_NonExistingUser_ReturnsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.GetInstructorIdByUserIdAsync(userId);

            // Assert
            Assert.That(result,Is.Null);
        }

        [Test]
        public async Task AddAsync_ValidInstructorCertificate_AddsToDatabase()
        {
            // Arrange
            var certificate = new InstructorCertificate
            {
                InstructorId = 1,
                CertificateUrl = "https://example.com/cert",
                CertificateType = "Professional"
            };

            // Act
            var addedCertificate = await _repository.AddAsync(certificate);
            await _context.SaveChangesAsync();

            // Assert
            var certificateInDb = await _context.InstructorCertificates.FindAsync(addedCertificate.Id);
            Assert.That(certificateInDb, Is.Not.Null);
            Assert.That(certificateInDb.InstructorId,Is.EqualTo(certificate.InstructorId));
            Assert.That(certificateInDb.CertificateUrl, Is.EqualTo(certificate.CertificateUrl));
            Assert.That(certificateInDb.CertificateType, Is.EqualTo(certificate.CertificateType));
        }

        [Test]
        public async Task UpdateAsync_ValidInstructorCertificate_UpdatesDatabase()
        {
            // Arrange
            var certificate = new InstructorCertificate
            {
                InstructorId = 1,
                CertificateUrl = "https://example.com/cert",
                CertificateType = "Professional"
            };

            await _context.InstructorCertificates.AddAsync(certificate);
            await _context.SaveChangesAsync();

            // Act
            certificate.CertificateUrl = "https://example.com/updated-cert";
            var updatedCertificate = await _repository.UpdateAsync(certificate);
            await _context.SaveChangesAsync();

            // Assert
            var certificateInDb = await _context.InstructorCertificates.FindAsync(updatedCertificate.Id);
            Assert.That(certificateInDb, Is.Not.Null);
                Assert.That(certificateInDb.CertificateUrl, Is.EqualTo("https://example.com/updated-cert"));
        }

        [Test]
        public async Task DeleteAsync_ValidInstructorCertificate_RemovesFromDatabase()
        {
            // Arrange
            var certificate = new InstructorCertificate
            {
                InstructorId = 1,
                CertificateUrl = "https://example.com/cert",
                CertificateType = "Professional"
            };

            await _context.InstructorCertificates.AddAsync(certificate);
            await _context.SaveChangesAsync();

            // Act
            var deletedCertificate = await _repository.DeleteAsync(certificate);
            await _context.SaveChangesAsync();

            // Assert
            var certificateInDb = await _context.InstructorCertificates.FindAsync(deletedCertificate.Id);
            Assert.That(certificateInDb, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_WithFilter_ReturnsFilteredCertificates()
        {
            // Arrange
            var certificates = new[]
            {
                new InstructorCertificate { InstructorId = 1, CertificateType = "Professional" },
                new InstructorCertificate { InstructorId = 2, CertificateType = "Academic" },
                new InstructorCertificate { InstructorId = 3, CertificateType = "Professional" }
            };

            await _context.InstructorCertificates.AddRangeAsync(certificates);
            await _context.SaveChangesAsync();

            // Act
            var filteredCertificates = await _repository.GetAllAsync(
                filter: c => c.CertificateType == "Professional"
            );

            // Assert
            Assert.That(filteredCertificates.Count(), Is.EqualTo(2));
            Assert.That(filteredCertificates.All(c => c.CertificateType == "Professional"),Is.True);
        }

        [Test]
        public async Task GetAsync_WithFilter_ReturnsSingleCertificate()
        {
            // Arrange
            var certificates = new[]
            {
                new InstructorCertificate { InstructorId = 1, CertificateType = "Professional" },
                new InstructorCertificate { InstructorId = 2, CertificateType = "Academic" }
            };

            await _context.InstructorCertificates.AddRangeAsync(certificates);
            await _context.SaveChangesAsync();

            // Act
            var certificate = await _repository.GetAsync(
                filter: c => c.CertificateType == "Academic"
            );

            // Assert
            Assert.That(certificate, Is.Not.Null);
            Assert.That(certificate.CertificateType, Is.EqualTo("Academic"));
            Assert.That(certificate.InstructorId, Is.EqualTo(2));
        }
    }

    // Helper class to mock InstructorInfo for testing GetInstructorIdByUserIdAsync
}