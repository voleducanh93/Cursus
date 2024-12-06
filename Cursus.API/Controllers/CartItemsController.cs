using Cursus.Common.Helper;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("default")]

    public class CartItemsController : ControllerBase

    {
        private readonly ICartItemsService _cartItemsService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly APIResponse _response;
        public CartItemsController(ICartItemsService cartItemsService, APIResponse response, IUnitOfWork unitOfWork)
        {
            _cartItemsService = cartItemsService;
            _response = response;
            _unitOfWork = unitOfWork;
        }
        /// <summary>
        /// Delete Cart Items
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="401">Authenticate error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]
        public async Task<ActionResult<APIResponse>> DeleteCartItems(int id)
        {
            if (!ModelState.IsValid)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Result = ModelState;
                return BadRequest(_response);
            }
            var result = await _cartItemsService.DeleteCartItem(id);
            if (result == true)
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.Created;
                _response.Result = "Instructor registered successfully";
                return Ok(result);
            }

            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            return BadRequest(_response);
        }
        /// <summary>
        /// Get All Cart Items
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Authenticate error</response>
        [HttpPut("GetAll{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]
        public async Task<ActionResult<APIResponse>> GetAllCartItems(int id)
        {
            var result = await _cartItemsService.GetAllCartItems(id);
            if (result != null)
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = result;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            return BadRequest(_response);
        }

    }
}
