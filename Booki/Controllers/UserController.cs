using Booki.Services.Interfaces;
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
    }
}
