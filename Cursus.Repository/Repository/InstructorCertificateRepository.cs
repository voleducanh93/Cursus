using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class InstructorCertificateRepository : Repository<InstructorCertificate>, IInstructorCertificateRepository

    {
        private readonly CursusDbContext _db;
        public InstructorCertificateRepository(CursusDbContext db) : base(db) => _db = db;

        public async Task<int?> GetInstructorIdByUserIdAsync(Guid userId)
        {
            // Chuyển Guid sang string để so sánh
            var instructor = await _db.InstructorInfos
                .FirstOrDefaultAsync(i => i.UserId == userId.ToString());

            return instructor?.Id;
        }

    }

}
