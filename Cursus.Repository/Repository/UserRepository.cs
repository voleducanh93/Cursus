using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        private readonly CursusDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserRepository(CursusDbContext db, UserManager<ApplicationUser> userManager) : base(db)
        {
            _db = db;
            _userManager = userManager;
        }
        public async Task<ApplicationUser>? ExiProfile(string id)
        {
            return await GetAsync(filter: b => b.Id.Equals(id)) ?? throw new KeyNotFoundException("User is not found");
        }

        public async Task<bool> PhoneNumberExistsAsync(string phoneNumber)
        {
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            return user == null ? false : true;
        }

        public async Task<ApplicationUser> UpdProfile(ApplicationUser usr)
        {
            return await UpdateAsync(usr);
        }
        public async Task<bool> UsernameExistsAsync(string username)
        {
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                return false;
            }
            return true;


        }
    }
}
