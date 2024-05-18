using Booki.Helpers;
using Booki.Models.DTOs;
using Booki.Services.Interfaces;
using Booki.Wrappers;
using Booki.Wrappers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Booki.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    [Authorize]
    public class UserController : Controller
    {
        protected IUserService _userService;
        public UserController(IUserService userSerrvice)
        {
            _userService = userSerrvice;
        }

        [HttpGet("{userId}")]
        public IActionResult Index(int userId)
        {
            IResponse response = _userService.UserById(userId);

            return response.Success ?
                Ok(response) :
                BadRequest(response);
        }

        [HttpPatch("[Action]")]
        public IActionResult EditUser(UserDetailDTO userDetailDTO)
        {
            var userIdResponse = JWTHelper.GetUserIdFromHttpContext(HttpContext);
            if (!userIdResponse.Success)
                return BadRequest(userIdResponse);

            int userId = (userIdResponse as ComplexResponse<int>).Result;

            var editResponse = _userService.EditUser(userDetailDTO, userId);
            if(!editResponse.Success)
                return BadRequest(editResponse);

            return Ok(editResponse);
        }
    }
}
