using Booki.Helpers;
using Booki.Models;
using Booki.Models.DTOs;
using Booki.Services;
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
        protected IImageService _imageService;

        public UserController(IUserService userSerrvice, IImageService imageService)
        {
            _userService = userSerrvice;
            _imageService = imageService;
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
            IResponse response;

            response = JWTHelper.GetUserFromHttpContext(HttpContext);
            if (!response.Success)
                return BadRequest(response);

            User currentUser = (response as ComplexResponse<User>).Result;

            response = _userService.CheckIfNewUsernameIsAvailable(currentUser.Username, userDetailDTO.Username);
            if (!response.Success)
                return BadRequest(response);

            response = _imageService.SaveImage(currentUser.Username, userDetailDTO.ProfilePicture);
            if (!response.Success)
                return BadRequest(response);

            response = _userService.EditUser(userDetailDTO, currentUser.Id);
            if (!response.Success)
                return BadRequest(response);

            // TODO: Editar el nombre de la carpeta de usuario si fuese necesario.
            if (!currentUser.Username.Equals(userDetailDTO.Username))
            {

            }

            return Ok(response);
        }
    }
}
