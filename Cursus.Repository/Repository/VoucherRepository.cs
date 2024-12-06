using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class VoucherRepository : Repository<Voucher>, IVoucherRepository
    {
        private readonly CursusDbContext _context;

        public VoucherRepository(CursusDbContext db) : base(db)
        {
            _context = db;
        }

        public async Task<Voucher> GetByCodeAsync(string code)
        {
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(x => x.VoucherCode == code);
            if (voucher == null)
            {
                throw new Exception("Voucher not found");
            }
            return voucher;
        }

        public async Task<Voucher> GetByVourcherIdAsync(int voucherId)
        {
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(x => x.Id == voucherId);
            if (voucher == null)
            {
                throw new Exception("Voucher not found");
            }
            return voucher;
        }
    }
}
       