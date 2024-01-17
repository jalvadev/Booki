using Booki.Models;

namespace Booki.Repositories.Interfaces
{
    public interface IBookRepository : IDisposable
    {
        List<Book> GetBooksByUserId(int userId);

        Book InsertBook(Book book, int userId);

        Book UpdateBook(Book book);
    }
}
