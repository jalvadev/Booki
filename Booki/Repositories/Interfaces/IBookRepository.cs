using Booki.Models;

namespace Booki.Repositories.Interfaces
{
    public interface IBookRepository : IDisposable
    {
        List<Book> GetBooksByUserId(int userId);

        Book GetBookDetail(int bookId);

        Book InsertBook(Book book, int userId);

        Book UpdateBook(Book book);

        bool DeleteBook(int bookId);

        bool CheckBookBelongsToUser(int bookId, int userId);
    }
}
