namespace Booki.Models.DTOs
{
    public class BookDTO
    {
        public int Id { get; set; }

        public string CoverPicture { get; set; }

        public string Title { get; set; }

        public DateTime FinishDate { get; set; }

        public short Rating { get; set; }

        public string? Commentary { get; set; }
    }
}
