using Cursus.Data.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IStepCommentService
    {
        Task<IEnumerable<StepCommentDTO>> GetStepCommentsAsync(int stepId);
        Task<StepCommentDTO> PostStepComment(StepCommentCreateDTO stepComment);
        Task<bool> DeleteStepComment(int commentId);
        Task<bool> DeleteStepCommentIfAdmin(int commentId, string adminId);
    }
}