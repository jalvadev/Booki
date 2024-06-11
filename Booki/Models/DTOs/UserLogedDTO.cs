namespace Booki.Models.DTOs
{
    public class UserLogedDTO
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string ProfilePictureName { get { return $"/images/{Username}/profilepicture.jpg"; } }

        public string Token { get; set; }

        public Guid VerificationToken { get; set; }
    }
}
