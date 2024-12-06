using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface ICertificateService
    {
        public Task<byte[]> CreateCertificate(int courseId, string userId);
        public Task<byte[]> GetCertificatePdfByIdAsync(int certificateId);
        public Task<byte[]> ExportCertificatesToExcel();
        public Task<byte[]> ExportCertificatesUserToExcel(string id);
        public Task<byte[]> GetCertificatePdfByUserAndCourseAsync(int courseId, string userId);
    }
}
