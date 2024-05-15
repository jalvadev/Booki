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
using Booki.Services.Interfaces;

namespace Booki.Controllers
{
    public class RegisterController : Controller
    {
        protected IConfiguration _configuration;
        protected IUserService _userService;
        protected IUserRepository _userRepository;
        protected IImageService _imageService;

        public RegisterController(IConfiguration configuration, IUserService userService, IUserRepository userRepository, IImageService imageService)
        {
            _configuration = configuration;
            _userService = userService;
            _userRepository = userRepository;
            _imageService = imageService;
        }

        [HttpPost("[action]")]
        public IActionResult Register(UserRegistrationDTO user)
        {
            IResponse response;

            response = RegistrationHelper.CheckUserRegistrationData(user, _userRepository);
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

        [HttpGet("VerifyAccount/{token}")]
        public IActionResult VerifyAccount(Guid token)
        {
            bool verified = _userRepository.SetUserVerification(token);

            IResponse response = verified ?
                new SimpleResponse { Success = true, Message = "Usuario verificado." } :
                new SimpleResponse { Success = false, Message = "Error al verificar el usuario." };

            return Ok(response);
        }

        #region Register Private Methods

        private IResponse RegisterUser(UserRegistrationDTO user)
        {
            IResponse response;

            response = _imageService.SaveImage(user.UserName, user.ProfilePicture);

            if (response.Success)
            {
                response = _userService.RegisterUser(user);
            }

            return response;
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

    }
}
