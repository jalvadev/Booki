using Booki.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Booki.Controllers
{
    public class AuthController : ControllerBase
    {
        public AuthController() { }

        public IActionResult Login(UserLoginDTO user)
        {
            return Ok(user);
        }
    }
}
