namespace Booki.Models.DTOs
{
    public class UserPasswordDTO
    {
        public string OldPassword { get; set; }

        public string NewPassword { get; set; }

        public string NewPassowrdConfirm { get; set; }
    }
}
