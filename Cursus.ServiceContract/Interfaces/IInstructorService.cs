using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IInstructorService
    {
        Task<ApplicationUser> InstructorAsync(RegisterInstructorDTO registerInstructorDTO);
        Task<bool> ApproveInstructorAsync(int instructorId);
        Task<bool> RejectInstructorAsync(int instructorId);
        Task<InstuctorTotalEarnCourseDTO> GetTotalAmountAsync (int instructorId);
        Task<IEnumerable<InstructorInfo>> GetAllInstructors();
       

    }
}

