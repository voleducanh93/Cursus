using Cursus.Data.DTO;
using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO);
        Task LogoutAsync(string refreshToken);
        public Task<ApplicationUser> RegisterAsync(UserRegisterDTO dto);
        Task<bool> ConfirmEmail(string username, string token);
        Task<RefreshToken> GetRefreshTokenAsync(ApplicationUser user);
        Task<LoginResponseDTO> RefreshTokenAsync(string token);
        Task<bool> ForgetPassword(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    }
}
