using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
namespace Cursus.Repository.Repository
{
    public class AdminRepository : Repository<ApplicationUser>, IAdminRepository
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly CursusDbContext db;


        public AdminRepository(CursusDbContext db, UserManager<ApplicationUser> userManager) : base(db)
        {
            this.userManager = userManager;
            this.db = db;
        }


        public async Task<bool> ToggleUserStatusAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            user.Status = !user.Status;

            var result = await userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> AdminComments(string userId, string commentText)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            var adminComment = new AdminComment
            {
                UserId = user.Id,
                CommentText = commentText,
                CommenterId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await db.AdminComments.AddAsync(adminComment);
            var result = await db.SaveChangesAsync();

            return result > 0;

        }

        public async Task<(string? UserName, string? Email, string? PhoneNumber, string? AdminComment, double TotalEarning )> GetInformationInstructorAsync(int instructorId)
        {
            var instructorInfo = await db.InstructorInfos.FirstOrDefaultAsync(i => i.Id == instructorId);
            if (instructorInfo != null)
            {
                var user = await db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == instructorInfo.UserId);
                var comment = await db.AdminComments
                                          .Where(c => c.UserId == instructorInfo.UserId)
                                          .OrderByDescending(c => c.CreatedAt)
                                          .FirstOrDefaultAsync();
                if (user != null)
                {
                    return (user.UserName, user.Email, user.PhoneNumber, comment?.CommentText, instructorInfo.TotalEarning);
                }
            }

            // Trả về giá trị mặc định nếu không tìm thấy
            return (null, null, null, null,0);
        }


    }
}



