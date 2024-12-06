using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorCertificateController : ControllerBase
    {
        private readonly IInstructorCertificateService _instructorCertificateService;

        public InstructorCertificateController(IInstructorCertificateService instructorCertificateService)
        {
            _instructorCertificateService = instructorCertificateService;
        }

        /// <summary>
        /// Upload certificate
        /// </summary>
        /// <param name="files"></param>
        /// <param name="certificateType"></param>
        /// <returns></returns>
        [HttpPost("upload-certificate")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor")]
        public async Task<IActionResult> UploadCertificate([FromForm] List<IFormFile> files, [FromForm] string certificateType)
        {
            try
            {
                // Lấy UserId từ Claim
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized("Invalid or missing token.");

                var uploadedFileUrls = await _instructorCertificateService.UploadCertificatesAsync(userId, files, certificateType);

                return Ok(new
                {
                    Message = "Files uploaded and information saved successfully.",
                    FileUrls = uploadedFileUrls
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
