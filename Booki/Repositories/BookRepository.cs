using Booki.Models;
using Booki.Models.Context;
using Booki.Repositories.Interfaces;

namespace Booki.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly BookiContext _bookiContext;
        private bool _disposed;

        public BookRepository(BookiContext bookiContext)
        {
            _bookiContext = bookiContext;
            _disposed = false;
        }


        public Book InsertBook(Book book, int userId)
        {
            Book insertedBook = new Book();

            try
            {
                Bookshelf bookshelf = _bookiContext.Users.Where(u => u.Id == userId).Select(u => u.Bookshelf).FirstOrDefault();
                if(bookshelf.Books == null)
                    bookshelf.Books = new List<Book>();

                bookshelf.Books.Add(book);

                var result = _bookiContext.Update(bookshelf);
                insertedBook = result.Entity.Books.FirstOrDefault();

                Save();
            }catch(Exception ex)
            {
                insertedBook = null;
            }

            return insertedBook;
        }
        public void Save()
        {
            _bookiContext.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed && disposing)
                _bookiContext.Dispose();

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
