using Booki.Models.DTOs;
using Booki.Wrappers.Interfaces;

namespace Booki.Services.Interfaces
{
    public interface IUserService
    {
        IResponse RegisterUser(UserRegistrationDTO user);

        IResponse UserById(int id);

        IResponse EditUser(UserDetailDTO user, int userId);

        IResponse CheckIfNewUsernameIsAvailable(string username, string newUsername);

        IResponse UpdateUserPassword(int userId, string password);

        IResponse CheckUserPassword(string username, string password);
    }
}
