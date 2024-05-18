using AutoMapper;
using Booki.Models;
using Booki.Models.DTOs;
using Booki.Repositories.Interfaces;
using Booki.Services.Interfaces;
using Booki.Wrappers;
using Booki.Wrappers.Interfaces;
using Salty.Hashers;
using Salty;

namespace Booki.Services
{
    public class UserService : IUserService
    {
        protected IUserRepository _userRepository;
        protected IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public IResponse RegisterUser(UserRegistrationDTO user)
        {
            IResponse response;

            User userToRegister = MapRegisterUser(user);
            userToRegister = _userRepository.RegisterUser(userToRegister);
            if (userToRegister == null)
                response = new SimpleResponse { Success = false, Message = "Ha ocurrido un error al registrar el usuario." };
            else
            {
                UserLogedDTO registeredUser = _mapper.Map<UserLogedDTO>(userToRegister);
                response = new ComplexResponse<UserLogedDTO> { Success = true, Message = "Usuario registrado.", Result = registeredUser };
            }

            return response;
        }

        public IResponse UserById(int id)
        {
            IResponse response;

            User user = _userRepository.UserById(id);
            if (user == null)
                response = new SimpleResponse { Success = false, Message = "Ha ocurrido un error al obtener el usuario." };
            else
            {
                UserDetailDTO registeredUser = _mapper.Map<UserDetailDTO>(user);
                response = new ComplexResponse<UserDetailDTO> { Success = true, Message = "Usuario obtenido.", Result = registeredUser };
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
    }
}
