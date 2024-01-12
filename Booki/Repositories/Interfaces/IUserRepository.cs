using Booki.Models;

namespace Booki.Repositories.Interfaces
{
    public interface IUserRepository : IDisposable
    {
        User LoginUser(string username, string password);
        User RegisterUser(User user);
    }
}
