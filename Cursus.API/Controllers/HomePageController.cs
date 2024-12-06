using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomePageController : ControllerBase
    {
        private readonly IHomePageService _homePageService;
        private readonly APIResponse _response;

        public HomePageController(IHomePageService homePageService, APIResponse response)
        {
            _homePageService = homePageService;
            _response = response;
        }

        /// <summary>
        /// Get home page information
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse>> GetHomePage()
        {
            var homePageInfo = await _homePageService.GetHomePageAsync();
            if (homePageInfo != null)
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = homePageInfo;
                return Ok(_response);
            }

            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.NotFound;
            _response.ErrorMessages.Add("Home page information not found.");
            return NotFound(_response);
        }

        /// <summary>
        /// Update home page information
        /// </summary>
        /// <param name="id"></param>
        /// <param name="homePageDto"></param>
        /// <returns></returns>
        //[Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [Authorize(AuthenticationSchemes = "Bearer", Policy = "FPTAdminOnly")]
        [HttpPut("{id}")]
        public async Task<ActionResult<APIResponse>> UpdateHomePage(int id, [FromBody] HomePageDTO homePageDto)
        {
            var updatedHomePage = await _homePageService.UpdateHomePageAsync(id, homePageDto);
            if (updatedHomePage != null)
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = updatedHomePage;
                return Ok(_response);
            }

            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.ErrorMessages.Add("Failed to update home page information.");
            return BadRequest(_response);

        }
    }
}
