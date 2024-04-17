using Booki.Models.DTOs;
using Booki.Wrappers.Interfaces;
using Booki.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Booki.Helpers;
using Booki.Models;
using Microsoft.Extensions.Configuration;
using Salty.Hashers;
using Salty;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using Booki.Repositories.Interfaces;
using AutoMapper;

namespace Booki.Controllers
{
    public class RegisterController : Controller
    {
        protected IConfiguration _configuration;
        protected IUserRepository _userRepository;
        protected IMapper _mapper;

        public RegisterController(IConfiguration configuration, IUserRepository userRepository, IMapper mapper)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _mapper = mapper;
        }

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

            if (response.Success)
                response = CheckUserEmailIsCorrect(user);

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

            return isTaken ? new SimpleResponse { Success = false, Message = "El nombre de usuario ya está en uso." }
                : new SimpleResponse { Success = true, Message = "El nombre de usuario está libre." };
        }

        private IResponse CheckUserEmailIsCorrect(UserRegistrationDTO user)
        {
            IResponse response;

            bool isTaken = _userRepository.CheckIfEmailIsAvailable(user.Email);

            return isTaken ? new SimpleResponse { Success = false, Message = "El email ya está en uso." }
            : new SimpleResponse { Success = true, Message = "El email está libre." };
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
            userToRegister.VerificationToken = Guid.NewGuid();

            var result = PasswordManager.GeneratePasswordHash(new SHA512Hasher(), user.Password);
            userToRegister.Password = result.HashedPassword;
            userToRegister.Salt = result.Salt;


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

            IResponse response = verified ?
                new SimpleResponse { Success = true, Message = "Usuario verificado." } :
                new SimpleResponse { Success = false, Message = "Error al verificar el usuario." };

            return Ok(response);
        }
    }
}
