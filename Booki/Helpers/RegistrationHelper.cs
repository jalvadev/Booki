﻿using Booki.Models.DTOs;
using Booki.Wrappers.Interfaces;
using Booki.Wrappers;
using System.Text.RegularExpressions;
using Booki.Repositories.Interfaces;

namespace Booki.Helpers
{
    public static class RegistrationHelper
    {
        private static IUserRepository _userRepository;

        public static IResponse CheckUserRegistrationData(UserRegistrationDTO user, IUserRepository userRepository)
        {
            IResponse response;
            _userRepository = userRepository;

            response = CheckRegisterMandatoryFields(user);

            if (response.Success)
                response = CheckPasswordIsCorrect(user);

            if (response.Success)
                response = CheckUserNameIsCorrect(user);

            if (response.Success)
                response = CheckUserEmailIsCorrect(user);

            return response;
        }

        private static IResponse CheckRegisterMandatoryFields(UserRegistrationDTO user)
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

        private static IResponse CheckPasswordIsCorrect(UserRegistrationDTO user)
        {
            IResponse response;

            response = IsUserPasswordValid(user.Password);

            if (response.Success)
                response = ConfirmationPassIsCorrect(user.Password, user.ConfirmationPassword);

            return response;
        }

        public static IResponse IsUserPasswordValid(string password)
        {
            IResponse response;

            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$";
            bool result = Regex.IsMatch(password, pattern);

            response = result ? new SimpleResponse { Success = true, Message = "La contraseña es válida." }
            : new SimpleResponse { Success = false, Message = "La contraseña debe tener al menos: 8 carácteres de largo, una letra mayúscula, otra minúscula y al menos un número." };

            return response;
        }

        public static IResponse ConfirmationPassIsCorrect(string pass1, string pass2)
        {
            IResponse response;

            if (pass1.Equals(pass2))
                response = new SimpleResponse { Success = true, Message = "Las contraseñas coinciden." };
            else
                response = new SimpleResponse { Success = false, Message = "Las contraseñas no coinciden." };

            return response;
        }

        private static IResponse CheckUserNameIsCorrect(UserRegistrationDTO user)
        {
            IResponse response;

            bool isAvailable = _userRepository.CheckIfUsernameIsAvailable(user.UserName);

            return !isAvailable ? new SimpleResponse { Success = false, Message = "El nombre de usuario ya está en uso." }
                : new SimpleResponse { Success = true, Message = "El nombre de usuario está libre." };
        }

        private static IResponse CheckUserEmailIsCorrect(UserRegistrationDTO user)
        {
            IResponse response;

            bool isAvailable = _userRepository.CheckIfEmailIsAvailable(user.Email);

            return !isAvailable ? new SimpleResponse { Success = false, Message = "El email ya está en uso." }
            : new SimpleResponse { Success = true, Message = "El email está libre." };
        }

        public static IResponse CheckRecoveryToken(Guid? token)
        {
            IResponse response;
            bool isOk;

            isOk = token.HasValue;
            if (!isOk)
                return new SimpleResponse { Success = false, Message = "Es necesario el token de recuperación." };

            isOk = token.Value is Guid;

            return !isOk ? new SimpleResponse { Success = false, Message = "El token no es válido." }
                : new SimpleResponse { Success = true, Message = "El token es válido." };
        }
    }
}
