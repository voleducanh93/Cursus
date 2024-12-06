using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IInstructorCertificateService
    {
        Task<List<string>> UploadCertificatesAsync(Guid userId, List<IFormFile> files, string certificateType);
    }
}
