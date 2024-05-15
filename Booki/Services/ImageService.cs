using Booki.Helpers;
using Booki.Models;
using Booki.Models.DTOs;
using Booki.Services.Interfaces;
using Booki.Wrappers;
using Booki.Wrappers.Interfaces;

namespace Booki.Services
{
    public class ImageService : IImageService
    {
        public IResponse SaveImage(string userName, string picture)
        {
            IResponse response;

            try
            {
                string userDirectoryPath = ImageHelper.CreateUserDirectoryIfNotExists(userName);

                byte[] coverBytes = ImageHelper.ConvertBase64OnBytes(picture);

                string currentBookPath = $"{userDirectoryPath}/profilepicture.jpg";

                bool saved = ImageHelper.SaveImage(currentBookPath, coverBytes);

                response = new SimpleResponse { Success = saved, Message = "Foto de perfil guardada." };
            }
            catch (Exception e)
            {
                response = new SimpleResponse { Success = false, Message = "Error al guardar la imagen." };
            }

            return response;
        }
    }
}
