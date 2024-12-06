using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Cursus.Data.Models;
using Cursus.Data.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Cursus.Repository.Repository;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class CertificateRepositoryTests
    {
        private CursusDbContext _context;
        private CertificateRepository _repository;
        private Course _testCourse;
        private ApplicationUser _testUser;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CursusDbContext>()
                .UseInMemoryDatabase(databaseName: $"CursusDb_{System.Guid.NewGuid()}")
                .Options;

            _context = new CursusDbContext(options);
            _repository = new CertificateRepository(_context);

            // Create test course
            _testCourse = new Course { Id = 1, Name = "Test Course" };
            _context.Courses.Add(_testCourse);

            // Create test user
            _testUser = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            _context.Users.Add(_testUser);

            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddAsync_ShouldAddCertificateAndReturnIt()
        {
            // Arrange
            var certificate = new Certificate
            {
                PdfData = new byte[] { 1, 2, 3, 4 },
                CourseId = _testCourse.Id,
                UserId = _testUser.Id,
                CreateDate = DateTime.Now
            };

            // Act
            var result = await _repository.AddAsync(certificate);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(await _context.Certificates.CountAsync(), Is.EqualTo(1));
            Assert.That(result.CourseId, Is.EqualTo(_testCourse.Id));
            Assert.That(result.UserId, Is.EqualTo(_testUser.Id));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveCertificateAndReturnIt()
        {
            // Arrange
            var certificate = new Certificate
            {
                PdfData = new byte[] { 1, 2, 3, 4 },
                CourseId = _testCourse.Id,
                UserId = _testUser.Id,
                CreateDate = DateTime.Now
            };
            await _context.Certificates.AddAsync(certificate);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(certificate);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(await _context.Certificates.CountAsync(), Is.EqualTo(0));
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateCertificateAndReturnIt()
        {
            // Arrange
            var certificate = new Certificate
            {
                PdfData = new byte[] { 1, 2, 3, 4 },
                CourseId = _testCourse.Id,
                UserId = _testUser.Id,
                CreateDate = DateTime.Now
            };
            await _context.Certificates.AddAsync(certificate);
            await _context.SaveChangesAsync();

            // Act
            var newPdfData = new byte[] { 5, 6, 7, 8 };
            certificate.PdfData = newPdfData;
            var result = await _repository.UpdateAsync(certificate);
            await _context.SaveChangesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.PdfData, Is.EqualTo(newPdfData));
            var updatedCertificate = await _context.Certificates.FirstAsync();
            Assert.That(updatedCertificate.PdfData, Is.EqualTo(newPdfData));
        }

        [Test]
        public async Task GetAsync_WithValidFilter_ShouldReturnCertificate()
        {
            // Arrange
            var certificate = new Certificate
            {
                PdfData = new byte[] { 1, 2, 3, 4 },
                CourseId = _testCourse.Id,
                UserId = _testUser.Id,
                CreateDate = DateTime.Now
            };
            await _context.Certificates.AddAsync(certificate);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAsync(c => c.CourseId == _testCourse.Id && c.UserId == _testUser.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.CourseId, Is.EqualTo(_testCourse.Id));
            Assert.That(result.UserId, Is.EqualTo(_testUser.Id));
        }

        [Test]
        public async Task GetAsync_WithInvalidFilter_ShouldReturnNull()
        {
            // Arrange
            var certificate = new Certificate
            {
                PdfData = new byte[] { 1, 2, 3, 4 },
                CourseId = _testCourse.Id,
                UserId = _testUser.Id,
                CreateDate = DateTime.Now
            };
            await _context.Certificates.AddAsync(certificate);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAsync(c => c.CourseId == 999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_WithNoFilter_ShouldReturnAllCertificates()
        {
            // Arrange
            var certificates = new List<Certificate>
            {
                new Certificate { PdfData = new byte[] { 1, 2, 3 }, CourseId = _testCourse.Id, UserId = _testUser.Id },
                new Certificate { PdfData = new byte[] { 4, 5, 6 }, CourseId = _testCourse.Id, UserId = _testUser.Id },
                new Certificate { PdfData = new byte[] { 7, 8, 9 }, CourseId = _testCourse.Id, UserId = _testUser.Id }
            };
            await _context.Certificates.AddRangeAsync(certificates);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task GetAllAsync_WithFilter_ShouldReturnFilteredCertificates()
        {
            // Arrange
            var certificates = new List<Certificate>
            {
                new Certificate { PdfData = new byte[] { 1, 2, 3 }, CourseId = _testCourse.Id, UserId = _testUser.Id },
                new Certificate { PdfData = new byte[] { 4, 5, 6 }, CourseId = 999, UserId = _testUser.Id },
                new Certificate { PdfData = new byte[] { 7, 8, 9 }, CourseId = _testCourse.Id, UserId = _testUser.Id }
            };
            await _context.Certificates.AddRangeAsync(certificates);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync(c => c.CourseId == _testCourse.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetAsync_WithIncludeProperties_ShouldReturnCertificateWithRelatedData()
        {
            // Arrange
            var certificate = new Certificate
            {
                PdfData = new byte[] { 1, 2, 3, 4 },
                CourseId = _testCourse.Id,
                UserId = _testUser.Id,
                CreateDate = DateTime.Now
            };
            await _context.Certificates.AddAsync(certificate);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAsync(
                c => c.CourseId == _testCourse.Id,
                includeProperties: "Course,ApplicationUser");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Course, Is.Not.Null);
            Assert.That(result.ApplicationUser, Is.Not.Null);
            Assert.That(result.Course.Id, Is.EqualTo(_testCourse.Id));
            Assert.That(result.ApplicationUser.Id, Is.EqualTo(_testUser.Id));
        }

        [Test]
        public async Task GetAllAsync_WithIncludeProperties_ShouldReturnCertificatesWithRelatedData()
        {
            // Arrange
            var certificates = new List<Certificate>
            {
                new Certificate { PdfData = new byte[] { 1, 2, 3 }, CourseId = _testCourse.Id, UserId = _testUser.Id },
                new Certificate { PdfData = new byte[] { 4, 5, 6 }, CourseId = _testCourse.Id, UserId = _testUser.Id }
            };
            await _context.Certificates.AddRangeAsync(certificates);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync(
                includeProperties: "Course,ApplicationUser");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(c => c.Course != null), Is.True);
            Assert.That(result.All(c => c.ApplicationUser != null), Is.True);
        }
    }
}