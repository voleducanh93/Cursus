using Cursus.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class PayoutRequestDisplayDTO
    {
        public int Id { get; set; }
        public string InstructorName { get; set; }
        public double Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public int TransactionId { get; set; }

        public PayoutRequestStatus Status { get; set; }
    }
}
