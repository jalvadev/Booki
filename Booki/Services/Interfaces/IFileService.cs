using Booki.Models.DTOs;
using Booki.Wrappers.Interfaces;

namespace Booki.Services.Interfaces
{
    public interface IFileService
    {
        IResponse SaveImage(string userName, string picture);

        IResponse SaveCoverImage(BookDTO newBook, string userName);

        IResponse UpdateCoverImage(BookDTO book, string userName);

        IResponse ChangeUsernameDirectoryName(string oldName, string newName);
    }
}
