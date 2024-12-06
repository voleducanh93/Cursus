using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class InstructorCertificateDto
    {
        public int InstructorId { get; set; } 
        public string CertificateType { get; set; }
        public List<string> FileUrls { get; set; }
    }

}
