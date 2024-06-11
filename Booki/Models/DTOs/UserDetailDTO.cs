namespace Booki.Models.DTOs
{
    public class UserDetailDTO
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string ProfilePicture { get; set; }

        public string ProfilePictureName { get { return $"/images/{Username}/profilepicture.jpg"; } }

    }
}
