﻿namespace Booki.Models.DTOs
{
    public class UserProfileDTO
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string ProfilePicture { get; set; }

        public string Token { get; set; }

        public Guid VerificationToken { get; set; }
    }
}
