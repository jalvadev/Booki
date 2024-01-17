using Booki.Models;

namespace Booki.Repositories.Interfaces
{
    public interface IBookRepository : IDisposable
    {
        Book InsertBook(Book book, int userId);

        Book UpdateBook(Book book);
    }
}
