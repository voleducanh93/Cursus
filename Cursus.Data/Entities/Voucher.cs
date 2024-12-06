using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class Voucher
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? VoucherCode { get; set; }
        public bool IsValid { get; set; } = true;
        public string? Name { get; set; }
        public DateTime CreateDate { get; set; } 
        public DateTime ExpireDate { get; set; }

        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        public double Percentage { get; set; }

        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
