using Booki.Models.DTOs;
using Booki.Wrappers.Interfaces;

namespace Booki.Services.Interfaces
{
    public interface IImageService
    {
        IResponse SaveImage(string userName, string picture);

        IResponse SaveCoverImage(BookDTO newBook, string userName);
    }
}
