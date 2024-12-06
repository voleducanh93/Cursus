using NUnit.Framework;
using Moq;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using System;
using Microsoft.Extensions.Configuration;

namespace Cursus.UnitTests.Services
{
    [TestFixture]
    public class EmailServiceTests
    {
        private Mock<IEmailRepository> _mockEmailRepository;
        private EmailService _emailService;
        private Mock<IConfiguration> _mockConfiguration;


        [SetUp]
        public void SetUp()
        {
            _mockEmailRepository = new Mock<IEmailRepository>();
            _mockConfiguration = new Mock<IConfiguration>();
            _emailService = new EmailService(_mockEmailRepository.Object, _mockConfiguration.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _mockEmailRepository = null;
            _mockConfiguration = null;
            _emailService = null;
        }

        #region SendEmail Tests

        [Test]
        public void SendEmail_WhenRequestIsValid_CallsRepositorySendEmail()
        {
            // Arrange
            var request = new EmailRequestDTO
            {
                toEmail = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            // Act
            _emailService.SendEmail(request);

            // Assert
            _mockEmailRepository.Verify(repo => repo.SendEmail(request), Times.Once);
        }

        [Test]
        public void SendEmail_WhenRepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var request = new EmailRequestDTO
            {
                toEmail = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };
            _mockEmailRepository.Setup(repo => repo.SendEmail(request)).Throws(new Exception("Repository error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _emailService.SendEmail(request));
            Assert.That(ex.Message, Is.EqualTo("Repository error"));
        }

        #endregion

        #region SendEmailConfirmation Tests

        [Test]
        public void SendEmailConfirmation_WhenParametersAreValid_CallsRepositorySendEmailConfirmation()
        {
            // Arrange
            var username = "test@example.com";
            var confirmLink = "http://example.com/confirm";

            // Act
            _emailService.SendEmailConfirmation(username, confirmLink);

            // Assert
            _mockEmailRepository.Verify(repo => repo.SendEmailConfirmation(It.Is<EmailRequestDTO>(r => r.toEmail == username && r.Subject == "Cursus Email Confirmation"), confirmLink), Times.Once);
        }

        #endregion

        #region SendEmailSuccessfullyPurchasedCourse Tests




        #endregion

        #region Performance Tests

        [Test, Timeout(1000)]
        public void SendEmail_PerformanceTest()
        {
            // Arrange
            var request = new EmailRequestDTO
            {
                toEmail = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            // Act
            _emailService.SendEmail(request);

            // Assert
            _mockEmailRepository.Verify(repo => repo.SendEmail(request), Times.Once);
        }

        #endregion

        #region Concurrency Tests

        [Test]
        public void SendEmail_ConcurrencyTest()
        {
            // Arrange
            var request = new EmailRequestDTO
            {
                toEmail = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            // Act
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => _emailService.SendEmail(request)));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert
            _mockEmailRepository.Verify(repo => repo.SendEmail(request), Times.Exactly(10));
        }

        #endregion
    }
}
