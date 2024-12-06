using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.IO;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("default")]
    public class StepContentController : ControllerBase
    {
        private readonly IStepContentService _stepContentService;
        private readonly IAzureBlobStorageService _azureBlobStorageService; // Inject Blob Storage Service
        private readonly APIResponse _response;

        public StepContentController(IStepContentService stepContentService, IAzureBlobStorageService azureBlobStorageService, APIResponse aPIResponse)
        {
            _stepContentService = stepContentService;
            _azureBlobStorageService = azureBlobStorageService; // Initialize Blob Storage Service
            _response = aPIResponse;
        }


        /// <summary>
        /// Upload file
        /// </summary>
        /// <param name="stepContentDTO"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor")]
        public async Task<ActionResult<APIResponse>> UploadFile([FromForm] StepContentDTO stepContentDTO, IFormFile file)
        {
            try
            {

                // check file
                if (file == null || file.Length == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("No file uploaded.");
                    return BadRequest(_response);
                }

                // up file lên Azure Blob Storage
                var blobUrl = await _azureBlobStorageService.UploadFileAsync(file);

                // save DTO
                stepContentDTO.ContentURL = blobUrl;
                stepContentDTO.ContentType = Path.GetExtension(file.FileName); // Lấy đuôi file

                // save
                var createdStepContentDTO = await _stepContentService.CreateStepContent(stepContentDTO);

                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.Created;
                _response.Result = createdStepContentDTO;
                return CreatedAtAction("GetStepContentById", new { id = createdStepContentDTO.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages.Add($"Internal Server Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }

        /// <summary>
        /// Get step content by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor,User")]
        public async Task<IActionResult> GetStepContentById(int id)
        {
            var stepContentDTO = await _stepContentService.GetStepContentByIdAsync(id);

            if (stepContentDTO == null)
            {
                return NotFound(new { message = "StepContent not found." });
            }

            return Ok(stepContentDTO);
        }

    }
}
