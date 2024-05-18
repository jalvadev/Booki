namespace Booki.Models.DTOs
{
    public class UserLogedDTO
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string ProfilePictureName { get; set; }

        public string Token { get; set; }

        public Guid VerificationToken { get; set; }
    }
}
