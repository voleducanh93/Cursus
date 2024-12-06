 using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class RegisterInstructorDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string? UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$",
        ErrorMessage = "Password must be at least 6 characters long, including at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password",ErrorMessage ="Passwords do not match")]
        public string? ConfirmPassword { get; set; }
        [Required(ErrorMessage ="Birthday is required")]
        public DateTime Birthday { get; set; }
        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage ="Invalid phone number")]
        public string? Phone { get; set; }
        [Required(ErrorMessage ="Address is required")]
        public string? Address { get; set; }
        [Required(ErrorMessage = "Tax number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Tax number must be exactly 10 digits")]
        public string? CardName { get; set; }
        [Required(ErrorMessage = "Card provider is required")]
        [StringLength(50, ErrorMessage = "Card provider cannot exceed 50 characters")]
        public string? CardProvider { get; set; }
        [Required(ErrorMessage = "Card number is required")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must be exactly 16 digits")]
        public string? CardNumber { get; set; }
        [Required(ErrorMessage = "Submit certificate is required")]
        public string? SubmitCertificate { get; set; }

        public double TotalEarning { get; set; } = 0;

        public double TotalWithdrawn { get; set; } = 0;
    }
}
