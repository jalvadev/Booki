using Booki.Models;

namespace Booki.Repositories.Interfaces
{
    public interface IUserRepository : IDisposable
    {
        User RegisterUser(User user);
    }
}
