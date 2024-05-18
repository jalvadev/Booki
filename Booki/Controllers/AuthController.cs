using AutoMapper;
using Booki.Helpers;
using Booki.Models;
using Booki.Models.DTOs;
using Booki.Repositories.Interfaces;
using Booki.Wrappers;
using Booki.Wrappers.Interfaces;
using Salty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Salty.Hashers;

namespace Booki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public AuthController(IConfiguration configuration, IUserRepository userRepository, IMapper mapper) 
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpPost("[action]")]
        public IActionResult Login([FromBody] UserLoginDTO user)
        {
            IResponse response;

            response = CheckUserLoginData(user);
            if(!response.Success)
                return BadRequest(response);

            response = LoginUser(user.UserName, user.Password);
            if(!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        #region Private Methods

        private IResponse CheckUserLoginData(UserLoginDTO user)
        {
            IResponse response;

            response = CheckLoginMandatoryFields(user);
            if (!response.Success)
                return response;

            response = CheckUserPassword(user);

            return response;
        }

        private IResponse CheckLoginMandatoryFields(UserLoginDTO user)
        {
            IResponse response;

            if (user == null)
                response = new SimpleResponse { Success = false, Message = "Faltan campos por rellenar." };
            else if (string.IsNullOrEmpty(user.UserName))
                response = new SimpleResponse { Success = false, Message = "El username no puede estar vacío." };
            else if (string.IsNullOrEmpty(user.Password))
                response = new SimpleResponse { Success = false, Message = "La contraseña es obligatoria." };
            else
                response = new SimpleResponse { Success = true, Message = "Todos los campos ok." };

            return response;
        }

        private IResponse CheckUserPassword(UserLoginDTO user)
        {
            IResponse response;

            Tuple<string, string> userPassAndSalt = _userRepository.GetUserSaltAndPass(user.UserName);
            if (userPassAndSalt == null)
                response = new SimpleResponse { Success = false, Message = "Hubo un error al obtener el usuario." };
            else
            {
                bool isCorrect = PasswordManager.ChekPasswordHash(new SHA512Hasher(), user.Password, userPassAndSalt.Item2, userPassAndSalt.Item1);

                if (isCorrect)
                    user.Password = userPassAndSalt.Item1;

                response = isCorrect ?
                    new SimpleResponse { Success = true, Message = "La contraseña es correcta." } :
                    new SimpleResponse { Success = false, Message = "La contraseña no es correcta." };
            }

            return response;
        }

        private IResponse LoginUser(string username, string password)
        {
            IResponse response;

            User user = _userRepository.LoginUser(username, password);
            if (user != null)
            {
                response = GetUserDTOWithToken(user);
            }
            else
            {
                response = new SimpleResponse { Success = false, Message = "Credenciales no validas." };
            }

            return response;
        }

        private IResponse GetUserDTOWithToken(User user)
        {
            UserLogedDTO userProfileDTO = _mapper.Map<UserLogedDTO>(user);
            userProfileDTO.Token = GetUserToken(user);

            return new ComplexResponse<UserLogedDTO> { Success = true, Message = "Usuario logeado.", Result = userProfileDTO }; ;
        }

        private string GetUserToken(User user)
        {
            var key = _configuration.GetValue<string>("Jwt:Key");
            var issuer = _configuration.GetValue<string>("Jwt:Issuer");
            var audience = _configuration.GetValue<string>("Jwt:Audience");

            string token = JWTHelper.GenerateToken(user, key, issuer, audience);

            return token;
        }
        #endregion
    }
}
