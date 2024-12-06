using Cursus.Data.DTO;
using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IStepService
    {
        Task<StepDTO> CreateStep(StepCreateDTO dto);
        Task<StepDTO> GetStepByIdAsync(int id);
        Task<bool> DeleteStep(int stepId);
        Task<IEnumerable<StepDTO>> GetStepsByCoursId(int courseId);
        Task<StepDTO> UpdateStep(StepUpdateDTO updateStepDTO);
        Task<TrackingProgressDTO> StartStepAsync(string userId, int stepId);
        Task<double> GetPercentageTrackingProgress(string userId, int coureseProgressId);

    }
}
