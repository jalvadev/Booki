namespace Booki.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public bool IsVerified { get; set; }

        public string ProfilePicture { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastUpdate { get; set; }

        public Bookshelf Bookshelf { get; set; }

        public Guid VerificationToken { get; set; }
    }
}
