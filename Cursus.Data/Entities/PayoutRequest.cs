using Cursus.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class PayoutRequest
    {
        public int Id { get; set; }
        public int InstructorId { get; set; }
        
        public int TransactionId { get; set; }
        public Transaction Transaction { get; set; }

        public DateTime CreatedDate { get; set; }

        public PayoutRequestStatus PayoutRequestStatus { get; set; } = PayoutRequestStatus.Pending;
    }
}
