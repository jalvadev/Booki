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
using Booki.Services.Interfaces;

namespace Booki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public AuthController(IConfiguration configuration, IUserRepository userRepository, IUserService userService, IMapper mapper) 
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _userService = userService;
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

        [HttpPost("[action]")]
        public IActionResult SendRestoreEmail([FromBody] string email)
        {
            bool emailExists = !_userRepository.CheckIfEmailIsAvailable(email);
            if (!emailExists)
                return BadRequest(new SimpleResponse { Success = false, Message = "El email no existe." });

            Guid token = Guid.NewGuid();
            bool tokenSet = _userRepository.SetUserVerificationToken(email, token);
            if (!tokenSet)
                return BadRequest(new SimpleResponse { Success = false, Message = "No se pudo generar el token de recuperación." });

            SendRecoveryEmail(email, token);

            return Ok(new SimpleResponse { Success = true, Message = "Se ha enviado un email para recuperar tu cuenta." });
        }

        [HttpPost("[action]")]
        public IActionResult RestorePassword([FromBody] UserPasswordDTO userPassword)
        {
            IResponse response;

            response = RegistrationHelper.IsUserPasswordValid(userPassword.NewPassword);
            if (!response.Success)
                return BadRequest(response);

            response = RegistrationHelper.CheckRecoveryToken(userPassword.RestoreToken);
            if (!response.Success)
                return BadRequest(response);

            response = RegistrationHelper.ConfirmationPassIsCorrect(userPassword.NewPassword, userPassword.NewPasswordConfirm);
            if (!response.Success)
                return BadRequest(response);

            response = _userService.RestoreUserPassword(userPassword.RestoreToken.Value, userPassword.NewPassword);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        // TODO : Clean this code ->
        private void SendRecoveryEmail(string userEmail, Guid token)
        {
            string host = _configuration.GetValue<string>("Email:host");
            int port = _configuration.GetValue<int>("Email:port");

            string username = _configuration.GetValue<string>("Email:user");
            string password = _configuration.GetValue<string>("Email:password");
            string from = _configuration.GetValue<string>("Email:from");
            string subject = _configuration.GetValue<string>("Email:subject");
            string body = _configuration.GetValue<string>("Email:bodyRecovery");

            body = body.Replace("##TOKEN##", token.ToString());

            SmtpClient client = new SmtpClient(host, port);

            MailMessage message = new MailMessage(from, userEmail, subject, body);
            message.IsBodyHtml = true;

            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(username, password);

            client.Send(message);
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
