using Cursus.Data.DTO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IStepContentService
    {
        Task<StepContentDTO> CreateStepContent(StepContentDTO stepContentDTO);
        Task<StepContentDTO> GetStepContentByIdAsync(int id);
        Task<StepContentDTO> CreateStepContentWithFileAsync(StepContentDTO stepContentDTO, IFormFile file);

    }
}
