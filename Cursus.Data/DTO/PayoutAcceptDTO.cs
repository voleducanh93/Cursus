using Cursus.Data.Entities;
using Cursus.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class PayoutAcceptDTO
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public int TransactionId { get; set; }

        public PayoutRequestStatus Status { get; set; }
        public InstructorInfo Instructor { get; set; }
    }
}
