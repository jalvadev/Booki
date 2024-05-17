using Booki.Models.DTOs;
using Booki.Wrappers.Interfaces;

namespace Booki.Services.Interfaces
{
    public interface IUserService
    {
        IResponse RegisterUser(UserRegistrationDTO user);

        IResponse UserById(int id);
    }
}
