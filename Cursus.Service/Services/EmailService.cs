using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace Cursus.Service.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailRepository _emailRepository;
        private readonly IConfiguration _configuration;

        public EmailService(IEmailRepository emailRepository, IConfiguration configuration)
        {
            _emailRepository = emailRepository;
            _configuration = configuration;
        }
        public void SendEmail(EmailRequestDTO request)
        {
            try
            {
                _emailRepository.SendEmail(request);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public void SendEmailConfirmation(string username, string confirmLink)
        {
            EmailRequestDTO request = new EmailRequestDTO
            {
                Subject = "Cursus Email Confirmation",
                Body = "",
                toEmail = username
            };
            _emailRepository.SendEmailConfirmation(request, confirmLink);

        }

        public void SendEmailSuccessfullyPurchasedCourse(ApplicationUser user, Order order)
        {

            EmailRequestDTO request = new EmailRequestDTO
            {
                Subject = "Order Confirmation Email",
                Body = "",
                toEmail = user.Email
            };

			_emailRepository.SendEmailSuccessfullyPurchasedCourse(request, order);
		}

        public void SendEmailCertificateCompletion(ApplicationUser user, string  url)
        {

            EmailRequestDTO request = new EmailRequestDTO
            {
                Subject = "Cursus Email Successfully with certificate",
                Body = "",
                toEmail = user.Email
            };

            _emailRepository.SendCertificateEmail(request,url );
        }

        

        public async Task SendEmailAsync(ApplicationUser user, string resetLink)
        {
            var emailRequest = new EmailRequestDTO
            {
                Subject = "Password Reset Request",
                toEmail = user.Email // Sử dụng email thực của người dùng
            };

             _emailRepository.SendEmailForgotPassword(emailRequest, resetLink);

        }

    }
}
