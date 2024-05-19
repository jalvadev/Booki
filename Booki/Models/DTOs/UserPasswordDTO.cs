namespace Booki.Models.DTOs
{
    public class UserPasswordDTO
    {
        public Guid? RestoreToken { get; set; }

        public string? OldPassword { get; set; }

        public string NewPassword { get; set; }

        public string NewPasswordConfirm { get; set; }
    }
}
