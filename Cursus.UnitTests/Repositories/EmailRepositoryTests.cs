using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.Repository.Repository;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MimeKit;
using Moq;
using NUnit.Framework;
using System;

namespace Cursus.UnitTests.Repositories
{
    [TestFixture]
    public class EmailRepositoryTests
    {
        private Mock<IOptions<EmailSetting>> _mockEmailSettings;
        private Mock<ILogger<EmailRepository>> _mockLogger;
        private Mock<SmtpClient> _mockSmtpClient;
        private EmailRepository _emailRepository;
        private EmailSetting _emailSetting;

        [SetUp]
        public void SetUp()
        {
            _emailSetting = new EmailSetting
            {
    Email= "cursus.course@gmail.com",
    Password= "kyss nufj ljwv zttk",
    Host= "smtp.gmail.com",
    DisplayName= "Cursus",
    Port= 587
            };

            _mockEmailSettings = new Mock<IOptions<EmailSetting>>();
            _mockEmailSettings.Setup(x => x.Value).Returns(_emailSetting);

            _mockLogger = new Mock<ILogger<EmailRepository>>();

            _mockSmtpClient = new Mock<SmtpClient>();

            _emailRepository = new EmailRepository(_mockEmailSettings.Object, _mockLogger.Object);
        }

        [Test]
        public void SendEmail_SendsEmailSuccessfully()
        {
            // Arrange
            var emailRequest = new EmailRequestDTO
            {
                toEmail = "recipient@example.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            // Act & Assert
            Assert.DoesNotThrow(() => _emailRepository.SendEmail(emailRequest));
        }

        [Test]
        public void SendEmail_LogsErrorOnFailure()
        {
            // Arrange
            var emailRequest = new EmailRequestDTO
            {
                toEmail = "invalid email",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            _mockSmtpClient.Setup(x => x.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SecureSocketOptions>(), It.IsAny<CancellationToken>()));

            // Act & Assert
            var ex = Assert.Throws<MimeKit.ParseException>(() => _emailRepository.SendEmail(emailRequest));
        }

        [Test]
        public void SendEmailConfirmation_SendsEmailWithConfirmationLink()
        {
            // Arrange
            var emailRequest = new EmailRequestDTO
            {
                toEmail = "recipient@example.com",
                Subject = "Email Confirmation"
            };
            var confirmLink = "https://example.com/confirm";

            // Act
            Assert.DoesNotThrow(() => _emailRepository.SendEmailConfirmation(emailRequest, confirmLink));

            // Assert
            Assert.That(emailRequest.Body.Contains(confirmLink), Is.True);
        }

        [Test]
        public void SendEmailSuccessfullyPurchasedCourse_SendsEmailWithOrderDetails()
        {
            // Arrange
            var emailRequest = new EmailRequestDTO
            {
                toEmail = "recipient@example.com",
                Subject = "Course Purchase Confirmation"
            };
            var order = new Order
            {
                Cart = new Cart { Total = 100.0 },
                discountAmount = 10.0
            };

            // Act
            Assert.DoesNotThrow(() => _emailRepository.SendEmailSuccessfullyPurchasedCourse(emailRequest, order));

            // Assert
            Assert.That(emailRequest.Body, Is.Not.Null);
        }

        [Test]
        public void SendCertificateEmail_SendsEmailWithCertificateLink()
        {
            // Arrange
            var emailRequest = new EmailRequestDTO
            {
                toEmail = "recipient@example.com",
                Subject = "Your Certificate"
            };
            var certificateDownloadUrl = "https://example.com/certificate";

            // Act
            Assert.DoesNotThrow(() => _emailRepository.SendCertificateEmail(emailRequest, certificateDownloadUrl));

            // Assert
            Assert.That(emailRequest.Body.Contains(certificateDownloadUrl), Is.True);
        }

        [Test]
        public void SendEmailForgotPassword_SendsEmailWithResetLink()
        {
            // Arrange
            var emailRequest = new EmailRequestDTO
            {
                toEmail = "recipient@example.com",
                Subject = "Reset Password"
            };
            var resetLink = "https://example.com/reset-password";

            // Act
            Assert.DoesNotThrow(() => _emailRepository.SendEmailForgotPassword(emailRequest, resetLink));

            // Assert
            Assert.That(emailRequest.Body.Contains(resetLink), Is.True);
        }
    }
}