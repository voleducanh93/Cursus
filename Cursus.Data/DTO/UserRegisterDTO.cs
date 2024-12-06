using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class UserRegisterDTO
    {
        [Required]
        [EmailAddress(ErrorMessage = "Please enter valid email address")]
        public string UserName { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[!@#$%^&*(),.?\:{}|<>]).+$", 
            ErrorMessage = "The password must contain at least one uppercase letter and one special character.")]
        public string Password { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

    }
}
