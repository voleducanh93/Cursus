using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class TermPolicyRepository : Repository<Term>, ITermPolicyRepository
    {
        private readonly CursusDbContext _db;

        public TermPolicyRepository(CursusDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
