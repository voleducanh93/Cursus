using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class UserDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address {  get; set; } = string.Empty;
        public string PhoneNumber {  get; set; } = string.Empty;

        public bool EmailConfirmed { get; set; }
    }
}
