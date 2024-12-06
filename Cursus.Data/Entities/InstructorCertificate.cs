using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class InstructorCertificate
    {
        public int Id { get; set; }
        public int InstructorId { get; set; }
        public string CertificateUrl { get; set; }
        public string CertificateType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
