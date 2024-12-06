using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface IEmailRepository
    {
        public void SendEmail(EmailRequestDTO request);
        public void SendEmailConfirmation(EmailRequestDTO request, string confirmLink);
		void SendEmailSuccessfullyPurchasedCourse(EmailRequestDTO request, Order order);
        void SendCertificateEmail(EmailRequestDTO request, string certificateDownloadUrl);
        void SendEmailForgotPassword(EmailRequestDTO request, string resetLink);

    }
}
