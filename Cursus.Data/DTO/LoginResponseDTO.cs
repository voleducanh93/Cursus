using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class LoginResponseDTO
    {
        public UserDTO? User { get; set; }
        public IEnumerable<string> Role { get; set; } = new List<string>();
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;

    }
}
