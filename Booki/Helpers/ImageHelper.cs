﻿namespace Booki.Helpers
{
    public static class ImageHelper
    {
        public static byte[] ConvertBase64OnBytes(string base64Image)
        {
            string[] pd = base64Image.Split(',');
            byte[] imageByter = Convert.FromBase64String(pd[1]);

            return imageByter;
        }

        public static string CreateUserDirectoryIfNotExists(string username)
        {
            string fullPathDirectory = $"/images/{username}";

            if(!Directory.Exists(fullPathDirectory))
                Directory.CreateDirectory(fullPathDirectory);

        
            return fullPathDirectory;
        }

        public static string CreateBooksDirectoryIfNotExists(string username)
        {
            string fullPathDirectory = $"/images/{username}/Books";

            if (!Directory.Exists(fullPathDirectory))
                Directory.CreateDirectory(fullPathDirectory);


            return fullPathDirectory;
        }

        public static bool SaveImage(string fileName, byte[] image)
        {
            File.WriteAllBytes(fileName, image);

            return true;
        }
    }
}
