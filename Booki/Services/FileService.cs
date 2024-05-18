using Booki.Helpers;
using Booki.Models.DTOs;
using Booki.Services.Interfaces;
using Booki.Wrappers;
using Booki.Wrappers.Interfaces;

namespace Booki.Services
{
    public class FileService : IFileService
    {
        public IResponse SaveImage(string userName, string picture)
        {
            IResponse response;

            try
            {
                string userDirectoryPath = FileHelper.CreateUserDirectoryIfNotExists(userName);

                byte[] coverBytes = FileHelper.ConvertBase64OnBytes(picture);

                string currentBookPath = $"{userDirectoryPath}/profilepicture.jpg";

                bool saved = FileHelper.SaveImage(currentBookPath, coverBytes);

                response = new SimpleResponse { Success = saved, Message = "Foto de perfil guardada." };
            }
            catch (Exception e)
            {
                response = new SimpleResponse { Success = false, Message = "Error al guardar la imagen." };
            }

            return response;
        }

        public IResponse SaveCoverImage(BookDTO newBook, string userName)
        {
            IResponse response;

            try
            {
                string userDirectoryPath = FileHelper.CreateUserDirectoryIfNotExists(userName);
                string booksDirectoryPath = FileHelper.CreateBooksDirectoryIfNotExists(userName);

                byte[] coverBytes = FileHelper.ConvertBase64OnBytes(newBook.CoverPicture);
                coverBytes = FileHelper.ConvertImageToJPG(coverBytes);

                string currentBookPath = $"{booksDirectoryPath}/{Guid.NewGuid()}.jpg";
                newBook.CoverPicture = currentBookPath;

                bool saved = FileHelper.SaveImage(currentBookPath, coverBytes);

                response = new SimpleResponse { Success = saved, Message = "Libro guardado." };
            }
            catch (Exception e)
            {
                response = new SimpleResponse { Success = false, Message = "Error al guardar la imagen" };
            }

            return response;
        }

        public IResponse ChangeUsernameDirectoryName(string oldName, string newName)
        {
            IResponse response;
            bool result = true;

            try
            {
                if(oldName != newName)
                {
                    result = FileHelper.ChangeUserDirectoryName(oldName, newName);
                }

                response = result ? 
                    new SimpleResponse { Success = true, Message = "Nombre de directorio cambiado." } :
                    new SimpleResponse { Success = false, Message = "Error al cambiar el nombre del directorio." };
            }
            catch (Exception e)
            {
                response = new SimpleResponse { Success = false, Message = "Error al cambiar el nombre del directorio" };
            }

            return response;
        }
    }
}
