namespace Booki.Models
{
    public class Book
    {
        public int Id { get; set; }

        public string CoverPicture { get; set; }

        public string Title { get; set; }

        public DateTime FinishDate { get; set; }

        public short Rating { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}
