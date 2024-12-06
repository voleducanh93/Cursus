using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO.Payment
{
    public class CreatePaymentRequest
    {
        [Required(ErrorMessage = "OrderId is required.")]       
        public int OrderId { get; set; }
        
       
    }
}
