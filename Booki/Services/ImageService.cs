﻿using Booki.Helpers;
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

        public IResponse SaveCoverImage(BookDTO newBook, string userName)
        {
            IResponse response;

            try
            {
                string userDirectoryPath = ImageHelper.CreateUserDirectoryIfNotExists(userName);
                string booksDirectoryPath = ImageHelper.CreateBooksDirectoryIfNotExists(userName);

                byte[] coverBytes = ImageHelper.ConvertBase64OnBytes(newBook.CoverPicture);

                string currentBookPath = $"{booksDirectoryPath}/{Guid.NewGuid()}.jpg";
                newBook.CoverPicture = currentBookPath;

                bool saved = ImageHelper.SaveImage(currentBookPath, coverBytes);

                response = new SimpleResponse { Success = saved, Message = "Libro guardado." };
            }
            catch (Exception e)
            {
                response = new SimpleResponse { Success = false, Message = "Error al guardar la imagen" };
            }

            return response;
        }
    }
}
