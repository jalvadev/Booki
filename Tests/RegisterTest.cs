using AutoMapper;
using Booki.Controllers;
using Booki.Models.DTOs;
using Booki.Repositories.Interfaces;
using Booki.Wrappers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    internal class RegisterTest
    {
        private RegisterController _registerController;
        private Mock<IConfiguration> _configuration;
        private Mock<IUserRepository> _userRepository;
        private Mock<IMapper> _mapper;

        [SetUp]
        public void Setup()
        {
            _configuration = new Mock<IConfiguration>();
            _userRepository = new Mock<IUserRepository>();
            _mapper = new Mock<IMapper>();

            _registerController = new RegisterController(_configuration.Object, _userRepository.Object, _mapper.Object);
        }

        [Test]
        public void RegisterUser_PassError_ReturnFalse()
        {
            // Arrange
            UserRegistrationDTO user = new UserRegistrationDTO()
            {
                UserName = "Test",
                Email = "test@email.com",
                Password = "123",
                ConfirmationPassword = "123",
                ProfilePicture = "asdf"
            };

            // Act
            var result = _registerController.Register(user) as BadRequestObjectResult;

            // Asserts
            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOf<IResponse>(result.Value);
            Assert.That((result.Value as IResponse).Success, Is.False);
            Assert.That((result.Value as IResponse).Message, Is.EqualTo("La contraseña debe tener al menos: 8 carácteres de largo, una letra mayúscula, otra minúscula y al menos un número."));
        }
    }
}
