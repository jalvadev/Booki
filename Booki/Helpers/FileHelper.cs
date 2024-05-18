using ImageMagick;
using Microsoft.OpenApi.Extensions;

namespace Booki.Helpers
{
    public static class FileHelper
    {
        public static byte[] ConvertBase64OnBytes(string base64Image)
        {
            int index;
            string[] pd = base64Image.Split(',');

            index = pd.Length > 1 ? 1 : 0;

            byte[] imageByter = Convert.FromBase64String(pd[index]);

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

        public static bool ChangeUserDirectoryName(string oldName, string newName)
        {
            bool result = false;
            
            try
            {
                string fullPathOldDirectory = $"/images/{oldName}";
                string fullPathNewDirectory = $"/images/{newName}";

                if (Directory.Exists(fullPathOldDirectory))
                {
                    Directory.Move(fullPathOldDirectory, fullPathNewDirectory);
                    result = true;
                }
            }
            catch (Exception) { result = false; }

            return result;
        }

        public static bool SaveImage(string fileName, byte[] image)
        {
            File.WriteAllBytes(fileName, image);

            return true;
        }

        public static string GetImageFormat(byte[] imageData)
        {
            string formatName;

            var image = new MagickImage(imageData);
            var format = image.Format;
            formatName = format.GetDisplayName();

            return formatName;

        }

        public static byte[] ConvertImageToJPG(byte[] imageData)
        {
            var image = new MagickImage(imageData);
            image.Format = MagickFormat.Jpg;

            return image.ToByteArray();
        }
    }
}
