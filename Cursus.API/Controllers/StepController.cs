using Azure;
using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StepController : ControllerBase
    {   
        private readonly IStepService _stepService;
        private readonly APIResponse _response;

        public StepController(IStepService stepService, APIResponse response)
        {
            _stepService = stepService;
            _response = response;
        }

        /// <summary>
        /// Create Step
        /// </summary>
        /// <param name="createStepDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor")]
        public async Task<ActionResult<APIResponse>> CreateStep([FromBody] StepCreateDTO createStepDTO)
        {
            if (!ModelState.IsValid)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(_response);
            }

            var stepDTO = await _stepService.CreateStep(createStepDTO);

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = stepDTO;

            return CreatedAtAction("GetStepById", new { id = stepDTO.Id }, _response);
        }

        /// <summary>
        /// Get step by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor,User")]
        public async Task<ActionResult<APIResponse>> GetStepById(int id)
        {
            var stepDTO = await _stepService.GetStepByIdAsync(id);
            if (stepDTO == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add("Step not found.");
                return NotFound(_response);
            }

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = stepDTO;

            return Ok(_response);
        }
        /// <summary>
        /// Delete step
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor")]
        public async Task<ActionResult<APIResponse>> DeleteStep(int id)
        {
            if (id <= 0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Invalid step ID.");
                return BadRequest(_response);
            }

            var result = await _stepService.DeleteStep(id);

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = result;
            return Ok(_response);

        }

        /// <summary>
        /// Get step by course Id
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [HttpGet("{courseId}/steps")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor,User")]
        public async Task<ActionResult<APIResponse>> GetStepsByCoursId(int courseId)
        {
            try
            {
                var stepsDTO = await _stepService.GetStepsByCoursId(courseId);

                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = stepsDTO;

                return Ok(_response);
            }
            catch (KeyNotFoundException ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add(ex.Message);

                return NotFound(_response);
            }
        }
        
        /// <summary>
        /// Update step
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateStepDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor")]
        public async Task<ActionResult<APIResponse>> UpdateStep(int id, [FromBody] StepUpdateDTO updateStepDTO)
        {
            if (id != updateStepDTO.Id)
            {
                return BadRequest("Step ID mismatch.");
            }

            var step = await _stepService.GetStepByIdAsync(id);
            if (step == null)
            {
                return NotFound(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = new List<string> { "Step not found." }
                });
            }
            //
            var updatedStep = await _stepService.UpdateStep(updateStepDTO);

            var response = new APIResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = updatedStep
            };

            return Ok(response);
        }
        /// <summary>
        /// Start new step for student
        /// </summary>
        /// <param name="stepId"></param>
        /// <returns></returns>
        [HttpPost("start-step")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]
        public async Task<ActionResult<APIResponse>> StartStepAsync(int stepId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || stepId <= 0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Invalid user ID or step ID.");
                return BadRequest(_response);
            }

            var trackingProgress = await _stepService.StartStepAsync(userId, stepId);

            if (trackingProgress == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Failed to start step.");
                return BadRequest(_response);
            }

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = trackingProgress;

            return Ok(_response);
        }

        /// <summary>
        /// Tracking progress for student
        /// </summary>
        /// <param name="coureseProgressId"></param>
        /// <returns></returns>
        [HttpGet("progress-percentage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]
        public async Task<ActionResult<APIResponse>> GetPercentageTrackingProgress( int coureseProgressId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId) || coureseProgressId <= 0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Invalid user ID or course progress ID.");
                return BadRequest(_response);
            }

            var percentage = await _stepService.GetPercentageTrackingProgress(userId, coureseProgressId);

            // Kiểm tra nếu percentage là null hoặc không hợp lệ
            if (percentage < 0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Failed to retrieve progress percentage.");
                return BadRequest(_response);
            }

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = percentage;

            return Ok(_response);
        }

    }
}
