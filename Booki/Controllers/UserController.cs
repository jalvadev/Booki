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
        protected IFileService _fileService;

        public UserController(IUserService userSerrvice, IFileService imageService)
        {
            _userService = userSerrvice;
            _fileService = imageService;
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
        public IActionResult EditUser([FromBody] UserDetailDTO userDetailDTO)
        {
            IResponse response;
            IResponse fileResponse;

            response = JWTHelper.GetUserFromHttpContext(HttpContext);
            if (!response.Success)
                return BadRequest(response);

            User currentUser = (response as ComplexResponse<User>).Result;

            response = _userService.CheckIfNewUsernameIsAvailable(currentUser.Username, userDetailDTO.Username);
            if (!response.Success)
                return BadRequest(response);

            response = _userService.EditUser(userDetailDTO, currentUser.Id);
            if (!response.Success)
                return BadRequest(response);

            fileResponse = _fileService.SaveImage(currentUser.Username, userDetailDTO.ProfilePicture);
            if (!response.Success)
                return BadRequest(fileResponse);

            fileResponse = _fileService.ChangeUsernameDirectoryName(currentUser.Username, userDetailDTO.Username);
            if (!response.Success)
                return BadRequest(fileResponse);

            return Ok(response);
        }

        [HttpPatch("[Action]")]
        public IActionResult EditPassword([FromBody] UserPasswordDTO userPassword)
        {
            IResponse response;

            response = JWTHelper.GetUserFromHttpContext(HttpContext);
            if (!response.Success)
                return BadRequest(response);

            User user = (response as ComplexResponse<User>).Result;

            response = _userService.CheckUserPassword(user.Username, userPassword.OldPassword);
            if(!response.Success)
                return BadRequest(response);

            response = RegistrationHelper.IsUserPasswordValid(userPassword.NewPassword);
            if(!response.Success)
                return BadRequest(response);

            response = RegistrationHelper.ConfirmationPassIsCorrect(userPassword.NewPassword, userPassword.NewPassowrdConfirm);
            if (!response.Success)
                return BadRequest(response);

            response = _userService.UpdateUserPassword(user.Id, userPassword.NewPassword);
            if(!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
