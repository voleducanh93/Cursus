using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.Data.Enum;
using Cursus.Data.Models;
using Cursus.Repository.Repository;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Service.Services
{
    public class InstructorService : IInstructorService
    {
        public readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        public readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IWalletService _walletService;
        public InstructorService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IEmailService emailService, IMapper mapper, IWalletService walletService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _mapper = mapper;
            _walletService = walletService;
        }

        public async Task<ApplicationUser> InstructorAsync(RegisterInstructorDTO registerInstructorDTO)
        {
            var context = new ValidationContext(registerInstructorDTO);
            var user = new ApplicationUser
            {
                UserName = registerInstructorDTO.UserName,
                Email = registerInstructorDTO.UserName,
                PhoneNumber = registerInstructorDTO.Phone,
                Address = registerInstructorDTO.Address,
                EmailConfirmed = false
            };

            var userResult = await _userManager.CreateAsync(user, registerInstructorDTO.Password);


            if (userResult.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Instructor");

                var instructorInfo = new InstructorInfo
                {
                    UserId = user.Id,
                    CardName = registerInstructorDTO.CardName,
                    CardProvider = registerInstructorDTO.CardProvider,
                    CardNumber = registerInstructorDTO.CardNumber,
                    SubmitCertificate = registerInstructorDTO.SubmitCertificate,
                    TotalEarning = registerInstructorDTO.TotalEarning,
                    TotalWithdrawn = registerInstructorDTO.TotalWithdrawn,
                    StatusInsructor = InstructorStatus.Pending
                };

                await _unitOfWork.InstructorInfoRepository.AddAsync(instructorInfo);
                await _unitOfWork.SaveChanges();

                return user;
            }

            return null;
        }

        public async Task<IdentityResult> ConfirmInstructorEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }
            return result;
        }

        public async Task<bool> ApproveInstructorAsync(int instructorId)
        {
            var instructorInfo = await _unitOfWork.InstructorInfoRepository.GetAsync(x => x.Id == instructorId);
            if (instructorInfo == null) throw new KeyNotFoundException("Instuctor not found");
            if (instructorInfo.StatusInsructor == InstructorStatus.Approved) throw new InvalidOperationException("Instructor already Approved.");
            instructorInfo.StatusInsructor = InstructorStatus.Approved;
            await _unitOfWork.InstructorInfoRepository.UpdateAsync(instructorInfo);
            await _walletService.CreateWallet(instructorInfo.UserId);
            await _unitOfWork.SaveChanges();

            var user = await _userManager.FindByIdAsync(instructorInfo.UserId);
            //email approve body
            string emailBody = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Instructor Account Approved</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                        .email-container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); }}
                        .email-header {{ background-color: #5cb85c; padding: 15px; text-align: center; color: #ffffff; border-radius: 10px 10px 0 0; }}
                        .email-header h1 {{ margin: 0; font-size: 24px; }}
                        .email-body {{ padding: 20px; font-size: 16px; line-height: 1.6; color: #333333; }}
                        .email-body h2 {{ color: #5cb85c; }}
                        .email-footer {{ text-align: center; padding: 10px; background-color: #f4f4f4; font-size: 12px; color: #777777; border-radius: 0 0 10px 10px; }}
                        .email-footer a {{ color: #5cb85c; text-decoration: none; }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='email-header'>
                            <h1>Instructor Account Approved</h1>
                        </div>
                        <div class='email-body'>
                            <h2>Dear {user.UserName},</h2>
                            <p>Congratulations! Your instructor account has been <strong>approved</strong> and activated.</p>
                            <p>You can now log in to the system and start creating and managing your courses.</p>
                            <p>If you have any questions or need assistance, feel free to reach out to us.</p>
                            <p>Best regards,</p>
                            <p><strong>The Cursus Team</strong></p>
                        </div>
                        <div class='email-footer'>
                            <p>If you have any questions, please contact us at <a href='mailto:cursus.course@gmail.com'>cursus.course@gmail.com</a>.</p>
                            <p>&copy; 2024 Cursus. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";
            var emailRequest = new EmailRequestDTO
            {
                toEmail = user.Email,
                Subject = "Instructor Account Approved",
                Body = emailBody,
            };
            _emailService.SendEmail(emailRequest);



            return true;
        }


        public async Task<bool> RejectInstructorAsync(int instructorId)
        {
            var instructorInfo = await _unitOfWork.InstructorInfoRepository.GetAsync(x => x.Id == instructorId);
            if (instructorInfo == null) throw new KeyNotFoundException("Instuctor not found");
            if (instructorInfo.StatusInsructor == InstructorStatus.Rejected) throw new InvalidOperationException("Instructor already Rejected.");
            instructorInfo.StatusInsructor = InstructorStatus.Rejected;
            await _unitOfWork.InstructorInfoRepository.UpdateAsync(instructorInfo);
            await _unitOfWork.SaveChanges();

            var user = await _userManager.FindByIdAsync(instructorInfo.UserId);
            //email reject body
            string emailBody = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Instructor Account Rejected</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                        .email-container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); }}
                        .email-header {{ background-color: #d9534f; padding: 15px; text-align: center; color: #ffffff; border-radius: 10px 10px 0 0; }}
                        .email-header h1 {{ margin: 0; font-size: 24px; }}
                        .email-body {{ padding: 20px; font-size: 16px; line-height: 1.6; color: #333333; }}
                        .email-body h2 {{ color: #d9534f; }}
                        .email-footer {{ text-align: center; padding: 10px; background-color: #f4f4f4; font-size: 12px; color: #777777; border-radius: 0 0 10px 10px; }}
                        .email-footer a {{ color: #d9534f; text-decoration: none; }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='email-header'>
                            <h1>Instructor Account Rejected</h1>
                        </div>
                        <div class='email-body'>
                            <h2>Dear {user.UserName},</h2>
                            <p>We regret to inform you that your instructor account registration has been <strong>rejected</strong>.</p>
                            <p>Please feel free to reach out to our support team if you have any questions or need further information.</p>
                            <p>Thank you for your interest in becoming an instructor with us.</p>
                            <p>Best regards,</p>
                            <p><strong>The Cursus Team</strong></p>
                        </div>
                        <div class='email-footer'>
                            <p>If you have any questions, please contact us at <a href='mailto:cursus.course@gmail.com'>cursus.course@gmail.com</a>.</p>
                            <p>&copy; 2024 Cursus. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";
            var emailRequest = new EmailRequestDTO
            {
                toEmail = user.Email,
                Subject = "Instructor Account Rejected",
                //Body = $"Dear {user.UserName},<br>Your instructor account registration has been rejected. Please contact support for further information."
                Body = emailBody,
            };
            _emailService.SendEmail(emailRequest);

            return true;
        }

        public async Task<InstuctorTotalEarnCourseDTO> GetTotalAmountAsync(int instructorId)
        {
            var instructorInfo = await _unitOfWork.InstructorInfoRepository.GetAsync(
                x => x.Id == instructorId,
                    includeProperties: "User");

            if (instructorInfo == null)
                throw new KeyNotFoundException("Instructor is not found");
            var courseCount = await _unitOfWork.CourseRepository.CountAsync(c => c.InstructorInfoId == instructorId); ;
            var wallet = await _unitOfWork.WalletRepository.GetAsync(x => x.UserId == instructorInfo.UserId);
            var summaryDTO = new InstuctorTotalEarnCourseDTO
            {
                Id = instructorInfo.Id,
                InstructorName = instructorInfo.User?.UserName,
                Earnings = wallet.Balance ?? 0,
                CourseCount = courseCount
            };

            return summaryDTO;
        }



        public Task<IEnumerable<InstructorInfo>> GetAllInstructors()
        {
            return _unitOfWork.InstructorInfoRepository.GetAllInstructors();
        }

        
    }
}
