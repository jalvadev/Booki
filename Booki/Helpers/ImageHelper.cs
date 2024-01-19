namespace Booki.Helpers
{
    public static class ImageHelper
    {
        public static byte[] ConvertBase64OnBytes(string base64Image)
        {
            byte[] imageByter = Convert.FromBase64String(base64Image);

            return imageByter;
        }

        public static string CreateUserDirectoryIfNotExists(string username)
        {
            string fullPathDirectory = $"/{username}";

            if(!Directory.Exists(fullPathDirectory))
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
