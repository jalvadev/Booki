using AutoMapper;
using Booki.Models;
using Booki.Models.DTOs;
using Booki.Repositories.Interfaces;
using Booki.Wrappers;
using Booki.Wrappers.Interfaces;
using Cryptolib;
using Microsoft.AspNetCore.Mvc;
using Npgsql.Replication;
using System.Text.RegularExpressions;

namespace Booki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public AuthController(IUserRepository userRepository, IMapper mapper) 
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
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
                UserProfileDTO userProfileDTO = _mapper.Map<UserProfileDTO>(user);
                response = new ComplexResponse<UserProfileDTO> { Success = true, Message = "Usuario logeado.", Result = userProfileDTO };
            }
            else
            {
                response = new SimpleResponse { Success = false, Message = "Credenciales no validas." };
            }

            return response;
        }
        #endregion

        [HttpPost]
        public IActionResult Register(UserRegistrationDTO user)
        {
            IResponse response;

            response = CheckUserRegistrationData(user);
            if (!response.Success)
                return BadRequest(response);

            response = RegisterUser(user);
            if(!response.Success)
                return BadRequest(response);

            return Ok(response);
        }


        #region Register Private Methods

        private IResponse CheckUserRegistrationData(UserRegistrationDTO user)
        {
            IResponse response;

            response = CheckRegisterMandatoryFields(user);

            if (response.Success)
                response = CheckPasswordIsCorrect(user);

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
            : new SimpleResponse { Success = false, Message = "La contraseña debe tener al menos: 8 carácteres de largo, una letra mayúscula, otra miunúscula y al menos un número." };

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

        private IResponse RegisterUser(UserRegistrationDTO user)
        {
            IResponse response;

            User userToRegister = MapRegisterUser(user);
            userToRegister = _userRepository.RegisterUser(userToRegister);
            if (userToRegister == null)
                response = new SimpleResponse { Success = false, Message = "Ha ocurrido un error al registrar el usuario." };
            else
            {
                UserLoginDTO registeredUser = _mapper.Map<UserLoginDTO>(userToRegister);
                response = new ComplexResponse<UserLoginDTO> { Success = true, Message = "Usuario registrado.", Result = registeredUser };
            }

            return response;
        }

        private User MapRegisterUser(UserRegistrationDTO user)
        {
            User userToRegister = _mapper.Map<User>(user);
            userToRegister.CreationDate = DateTime.Now;
            userToRegister.LastUpdate = DateTime.Now;
            userToRegister.ProfilePicture = "";
            userToRegister.Bookshelf = new Bookshelf();
            userToRegister.Password = Crypto.GenerateSHA512String(user.Password);

            return userToRegister;
        }
        #endregion
    }
}
