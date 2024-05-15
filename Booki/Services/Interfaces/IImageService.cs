using Booki.Wrappers.Interfaces;

namespace Booki.Services.Interfaces
{
    public interface IImageService
    {
        IResponse SaveImage(string userName, string picture);
    }
}
