using Cursus.Data.DTO;
using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IReasonService
    {
        Task<Reason> CreateReason(CreateReasonDTO reasonDTO);
        Task<ReasonDTO> GetReasonByIdAsync(int id);
        Task DeleteReasonAsync(int id);
    }
}
