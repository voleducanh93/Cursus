using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cursus.Data.Enum;
using Cursus.Data.Enums;
namespace Cursus.Data.Entities
{
     public class InstructorInfo
     {
        public int Id { get; set; }
        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; }

        public ApplicationUser? User { get; set; }
        public string? CardName { get; set; }
        public string? CardProvider { get; set; }
        public string? CardNumber { get; set; }
        public string? SubmitCertificate { get; set; } 

        public InstructorStatus StatusInsructor { get; set; }

        public double TotalEarning { get; set; } = 0;

        public double TotalWithdrawn { get; set; } = 0;
        public ICollection<Course> Courses { get; set; } = new List<Course>();

        public ICollection<PayoutRequest> WithdrawalHistory { get; set; } = new List<PayoutRequest>();
    }
}
