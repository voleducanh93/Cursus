using Cursus.Data.Entities;
using Cursus.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class PayoutDenyDTO
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public int TransactionId { get; set; }
        [Required]
        public string Reason { get; set; } 

        public PayoutRequestStatus Status { get; set; }
    }
}
