using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class VoucherDTO
    {
        public string? VoucherCode { get; set; }
        public bool IsValid { get; set; } = true;
        public string? Name { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public double Percentage { get; set; }
        
    }
}
