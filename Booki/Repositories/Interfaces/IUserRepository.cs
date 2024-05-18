using Booki.Models;

namespace Booki.Repositories.Interfaces
{
    public interface IUserRepository : IDisposable
    {
        User LoginUser(string username, string password);
        Tuple<string, string> GetUserSaltAndPass(string username);
        User RegisterUser(User user);
        User UserById(int id);
        User EditUser(User user);
        bool CheckIfUsernameIsAvailable(string username);
        bool CheckIfEmailIsAvailable(string email);
        bool SetUserVerification(Guid token);
    }
}
