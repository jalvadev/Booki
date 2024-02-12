using AutoMapper;
using Booki.Helpers;
using Booki.Models;
using Booki.Models.DTOs;
using Booki.Repositories.Interfaces;
using Booki.Wrappers;
using Booki.Wrappers.Interfaces;
using Cryptolib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

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

        [HttpPost]
        public IActionResult Login(UserLoginDTO user)
        {
            IResponse response;

            response = CheckUserLoginData(user);
            if(!response.Success)
                return BadRequest(response);

            EncryptPassword(ref user);

            response = LoginUser(user.UserName, user.Password);
            if(!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        #region Login Private Methods

        private IResponse CheckUserLoginData(UserLoginDTO user)
        {
            IResponse response;

            response = CheckLoginMandatoryFields(user);

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

        private void EncryptPassword(ref UserLoginDTO user)
        {
            user.Password = Crypto.GenerateSHA512String(user.Password);
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
            UserProfileDTO userProfileDTO = _mapper.Map<UserProfileDTO>(user);
            userProfileDTO.Token = GetUserToken(user);

            return new ComplexResponse<UserProfileDTO> { Success = true, Message = "Usuario logeado.", Result = userProfileDTO }; ;
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

        [HttpPost("[action]")]
        public IActionResult Register(UserRegistrationDTO user)
        {
            IResponse response;

            response = CheckUserRegistrationData(user);
            if (!response.Success)
                return BadRequest(response);

            response = RegisterUser(user);
            if (!response.Success)
                return BadRequest(response);

            if (response.Success)
            {
                var result = response as ComplexResponse<UserProfileDTO>;
                SendVerificationEmail(user.Email, result.Result.VerificationToken);
            }

            return Ok(response);
        }


        #region Register Private Methods

        private IResponse CheckUserRegistrationData(UserRegistrationDTO user)
        {
            IResponse response;

            response = CheckRegisterMandatoryFields(user);

            if (response.Success)
                response = CheckPasswordIsCorrect(user);

            if (response.Success)
                response = CheckUserNameIsCorrect(user);

            return response;
        }
        private IResponse CheckRegisterMandatoryFields(UserRegistrationDTO user)
        {
            IResponse response;

            if (user == null)
                response = new SimpleResponse { Success = false, Message = "Faltan campos por rellenar." };
            else if (string.IsNullOrEmpty(user.UserName))
                response = new SimpleResponse { Success = false, Message = "El username es obligatorio." };
            else if (string.IsNullOrEmpty(user.Email))
                response = new SimpleResponse { Success = false, Message = "El email es obligatorio." };
            else if (string.IsNullOrEmpty(user.Password))
                response = new SimpleResponse { Success = false, Message = "La contraseña es obligatoria." };
            else
                response = new SimpleResponse { Success = true, Message = "Todos los campos ok." };

            return response;
        }

        private IResponse CheckPasswordIsCorrect(UserRegistrationDTO user)
        {
            IResponse response;

            response = IsUserPasswordValid(user.Password);

            if (response.Success)
                response = ConfirmationPassIsCorrect(user);

            return response;
        }

        private IResponse IsUserPasswordValid(string password)
        {
            IResponse response;

            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$";
            bool result = Regex.IsMatch(password, pattern);

            response = result ? new SimpleResponse { Success = true, Message = "La contraseña es válida." } 
            : new SimpleResponse { Success = false, Message = "La contraseña debe tener al menos: 8 carácteres de largo, una letra mayúscula, otra minúscula y al menos un número." };

            return response;
        }

        private IResponse ConfirmationPassIsCorrect(UserRegistrationDTO user)
        {
            IResponse response;

            if (user.Password.Equals(user.ConfirmationPassword))
                response = new SimpleResponse { Success = true, Message = "Las contraseñas coinciden." };
            else
                response = new SimpleResponse { Success = false, Message = "Las contraseñas no coinciden." };

            return response;
        }

        private IResponse CheckUserNameIsCorrect(UserRegistrationDTO user)
        {
            IResponse response;

            bool isTaken = _userRepository.CheckIfUsernameIsAvailable(user.UserName);

            return isTaken ? new SimpleResponse { Success = false, Message = "El nombre de usuario ya está cogido." }
                : new SimpleResponse { Success = true, Message = "El nombre de usuario está libre." };
        }

        private IResponse RegisterUser(UserRegistrationDTO user)
        {
            IResponse response;

            response = UploadProfileImage(user);

            if (response.Success)
            {
                response = InsertUser(user);
            }

            return response;
        }

        private IResponse UploadProfileImage(UserRegistrationDTO user)
        {
            IResponse response;

            try
            {
                string userDirectoryPath = ImageHelper.CreateUserDirectoryIfNotExists(user.UserName);

                user.ProfilePicture = user.ProfilePicture.Replace("data:image/jpeg;base64,", "");
                user.ProfilePicture = user.ProfilePicture.Replace("data:image/png;base64,", "");

                byte[] coverBytes = ImageHelper.ConvertBase64OnBytes(user.ProfilePicture);

                string currentBookPath = $"{userDirectoryPath}/profilepicture.jpg";

                bool saved = ImageHelper.SaveImage(currentBookPath, coverBytes);

                response = new SimpleResponse { Success = saved, Message = "Foto de perfil guardada." };
            }
            catch (Exception e)
            {
                response = new SimpleResponse { Success = false, Message = "Error al guardar la imagen" };
            }

            return response;
        }

        private IResponse InsertUser(UserRegistrationDTO user)
        {
            IResponse response;

            User userToRegister = MapRegisterUser(user);
            userToRegister = _userRepository.RegisterUser(userToRegister);
            if (userToRegister == null)
                response = new SimpleResponse { Success = false, Message = "Ha ocurrido un error al registrar el usuario." };
            else
            {
                UserProfileDTO registeredUser = _mapper.Map<UserProfileDTO>(userToRegister);
                response = new ComplexResponse<UserProfileDTO> { Success = true, Message = "Usuario registrado.", Result = registeredUser };
            }

            return response;
        }

        private User MapRegisterUser(UserRegistrationDTO user)
        {
            User userToRegister = _mapper.Map<User>(user);
            userToRegister.CreationDate = DateTime.Now;
            userToRegister.LastUpdate = DateTime.Now;
            userToRegister.ProfilePicture = "profilepicture.jpg";
            userToRegister.Bookshelf = new Bookshelf();
            userToRegister.Password = Crypto.GenerateSHA512String(user.Password);
            userToRegister.VerificationToken = Guid.NewGuid();

            return userToRegister;
        }

        // TODO : Clean this code ->
        private void SendVerificationEmail(string userEmail, Guid token)
        {
            string host = _configuration.GetValue<string>("Email:host");
            int port = _configuration.GetValue<int>("Email:port");

            string username = _configuration.GetValue<string>("Email:user");
            string password = _configuration.GetValue<string>("Email:password");
            string from = _configuration.GetValue<string>("Email:from");
            string subject = _configuration.GetValue<string>("Email:subject");
            string body = _configuration.GetValue<string>("Email:body");

            body = body.Replace("##TOKEN##", token.ToString());

            SmtpClient client = new SmtpClient(host, port);

            MailMessage message = new MailMessage(from, userEmail, subject, body);
            message.IsBodyHtml = true;
            
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(username, password);
            
            client.Send(message);
        }
        #endregion

        [HttpGet("VerifyAccount/{token}")]
        public IActionResult VerifyAccount(Guid token)
        {
            bool verified = _userRepository.SetUserVerification(token);

            IResponse response = verified ? new SimpleResponse { Success = true, Message = "Usuario verificado." }
                : new SimpleResponse { Success = false, Message = "Error al verificar el usuario." };

            return Ok(response);
        }
    }
}
