using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO.Payment
{
    public class CapturePaymentRequest
    {
        [Required(ErrorMessage = "Token is required.")]
        [StringLength(100, ErrorMessage = "Token length can't be more than 100 characters.")]
        public string Token { get; set; }
		[Required(ErrorMessage = "PayId is required.")]

		public string? PayId { get; set; }
        
    }
}
