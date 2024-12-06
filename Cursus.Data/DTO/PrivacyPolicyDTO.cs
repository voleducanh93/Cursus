using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class PrivacyPolicyDTO
    {
        public string Content { get; set; }
        public string LastUpdatedBy { get; set; }    
        public DateTime LastUpdatedDate { get; set; } = DateTime.Now;
    }
}
