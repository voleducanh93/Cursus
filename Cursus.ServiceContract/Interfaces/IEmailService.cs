using Cursus.Data.DTO;
using Cursus.Data.Entities;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IEmailService
    {
        public void SendEmail(EmailRequestDTO request);
        public void SendEmailConfirmation(string username, string confirmLink);
		void SendEmailSuccessfullyPurchasedCourse(ApplicationUser user, Order order);
        void SendEmailCertificateCompletion(ApplicationUser user, string certificate);
        Task SendEmailAsync(ApplicationUser user, string resetLink);

    }
}
