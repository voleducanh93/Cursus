using Cursus.Data.DTO;
using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface ITermPolicyService
    {
        Task<IEnumerable<Term>> GetTermPolicyAsync();
        Task<TermPolicyDTO> CreateTermPolicyAsync(TermPolicyDTO termPolicyDTO);
        Task<TermPolicyDTO> UpdateTermPolicyAsync(int id, TermPolicyDTO termPolicyDTO);
        Task DeleteTermPolicyAsync(int id);
    }
}
