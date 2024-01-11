using Microsoft.EntityFrameworkCore;

namespace Booki.Models.Context
{
    public class BookiContext : DbContext
    {
        public BookiContext(DbContextOptions<BookiContext> options) :base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Book> Books { get; set; }

        public DbSet<Bookshelf> Bookshelves { get; set; }
    }
}
