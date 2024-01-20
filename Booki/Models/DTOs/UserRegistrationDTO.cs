namespace Booki.Models.DTOs
{
    public class UserRegistrationDTO
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmationPassword { get; set; }

        public string ProfilePicture { get; set; }
    }
}
