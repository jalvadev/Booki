using Booki.Models;
using Booki.Models.DTOs;
using Booki.Wrappers.Interfaces;

namespace Booki.Services.Interfaces
{
    public interface IBookService
    {
        IResponse InsertBook(BookDTO book, int userId);

        IResponse GetBookDetail(int bookId);

        IResponse ListBooksByUserId(int userId);

        IResponse UpdateBook(BookDTO book);

        IResponse DeleteBook(int bookId, int userId);

        IResponse CheckIfBookBelongToUser(int bookId, int userId);
    }
}
