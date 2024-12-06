using AutoMapper;
using Cursus.Common.Middleware;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Fpe;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper, RoleManager<IdentityRole> roleManager, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _roleManager = roleManager;
            _emailService = emailService;
        }
        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginRequestDTO.Username);
            if (user == null)
                throw new BadHttpRequestException("Username or password is incorrect!");

            var isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

            if (!isValid)
                throw new BadHttpRequestException("Username or password is incorrect!");

            if (!user.Status)
                throw new BadHttpRequestException("Your account is banned!");

            if (!user.EmailConfirmed)
                return null;


            var token = await GenerateJwtToken(user);
            var refreshToken = await GetRefreshTokenAsync(user);
            var userDTO = _mapper.Map<UserDTO>(user);

            var role = await _userManager.GetRolesAsync(user); 

            LoginResponseDTO responseDTO = new()
            {
                User = userDTO,
                Token = token,
                RefreshToken = refreshToken.Token,
                Role = role
            };
            
            return responseDTO;
        }
        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id), //Thông tin chủ thể của object: tên đăng nhập của user
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),// unique identifier giúp phân biệt các token khác nhau, Sử dụng NewGuid() để tạo ra một giá trị đi nhất
                new Claim(ClaimTypes.NameIdentifier, user.Id), //Id để xác định người dùng 1 cách duy nhất 
                new Claim(ClaimTypes.Email, user.Email),
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<ApplicationUser> RegisterAsync(UserRegisterDTO dto)
        {
            var phoneNumberExisted = await _unitOfWork.UserRepository.PhoneNumberExistsAsync(dto.PhoneNumber);

            var userExisted = await _unitOfWork.UserRepository.UsernameExistsAsync(dto.UserName);

            if (userExisted)
            {
                throw new BadHttpRequestException("Username is existed");
            }

            if (phoneNumberExisted)
            {
                throw new BadHttpRequestException("Phone number is existed");
            }

            var user = _mapper.Map<ApplicationUser>(dto);   
            
            var result = await _userManager.CreateAsync(user, dto.Password);
            
            if (result.Succeeded == true)
            {

                if(dto.Role == "Instructor" || dto.Role == "Admin" || dto.Role == "User")
                {
                    if (!await _roleManager.RoleExistsAsync(dto.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(dto.Role));
                    }

                    await _userManager.AddToRoleAsync(user, dto.Role);
                }
                else
                {
                    var userToDelete = await _userManager.FindByEmailAsync(user.UserName);

                    await _userManager.DeleteAsync(userToDelete);

                    throw new BadHttpRequestException("Role is not valid");
                }

                await _unitOfWork.SaveChanges();

                var userForReturn = await _userManager.FindByEmailAsync(user.Email);

                return user;
            }
            else
            {
                throw new Exception("User is not created");
            }
        }

        public async Task<bool> ConfirmEmail(string username, string token)
        {
            var user = await _userManager.FindByEmailAsync(username) ?? throw (new BadHttpRequestException("User not found"));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(ApplicationUser user)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                UserId = user.Id,
                Expries = DateTime.UtcNow.AddDays(7),//Refresh hết hạn sau 7 ngày
                Created = DateTime.UtcNow
            };
            await _unitOfWork.RefreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.SaveChanges();   
            return refreshToken;
        }

        public async Task<LoginResponseDTO> RefreshTokenAsync(string token)
        {
            var findToken = await _unitOfWork.RefreshTokenRepository.GetRefreshTokenAsync(token);
            if (findToken == null || !findToken.IsActive)
                throw new BadHttpRequestException("Invalid or expried refresh token");

            var user = await _userManager.FindByIdAsync(findToken.UserId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var newAccessToken = await GenerateJwtToken(user);

            var newRefreshToken = await GetRefreshTokenAsync(user);
            findToken.Revoked = DateTime.UtcNow;
            await _unitOfWork.RefreshTokenRepository.UpdateAsync(findToken);
            await _unitOfWork.SaveChanges();
            return new LoginResponseDTO
            {
                User = _mapper.Map<UserDTO>(user),
                Token = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                Role = await _userManager.GetRolesAsync(user),
            };

        }

        public async Task LogoutAsync(string refreshToken)
        {
            var findToken = await _unitOfWork.RefreshTokenRepository.GetRefreshTokenAsync(refreshToken);
            if (findToken == null || !findToken.IsActive)
                throw new Exception("Invalid or expired refresh token");

            findToken.Revoked = DateTime.UtcNow;

            await _unitOfWork.RefreshTokenRepository.UpdateAsync(findToken);
            await _unitOfWork.SaveChanges();
        }

        public async Task<bool> ForgetPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new Exception("User not found");
            if (!user.Status)
            {
                throw new Exception("User is banned and cannot reset the password.");
            }

            var lifespanMinutes = int.Parse(_configuration["TokenSettings:PasswordResetTokenLifespan"]);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{_configuration["AppSettings:FrontendUrl"]}/reset-password?email={user.Email}&token={token}"; // nếu muốn sử an toàn trong URL thì nên mã hóa Uri.EscapeDataString(token)

            var emailRequest = new EmailRequestDTO
            {
                toEmail = email,
                Subject = "Password Reset",
                Body = $"Click vào link để đặt lại mật khẩu: {resetLink}"
            };

            await _emailService.SendEmailAsync(user, resetLink);

            return true;
        }


        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Đặt lại mật khẩu
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
                throw new BadHttpRequestException("Reset password failed. Token may be invalid or expired.");

            return true;
        }

    }
}

