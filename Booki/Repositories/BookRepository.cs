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

        public List<Book> GetBooksByUserId(int userId)
        {
            List<Book> userBooks;

            try
            {
                userBooks = _bookiContext.Users.Where(u => u.Id == userId).Select(u => u.Bookshelf.Books).FirstOrDefault();

            }catch(Exception ex)
            {
                userBooks = null;
            }

            return userBooks;
        }

        public Book GetBookDetail(int bookId)
        {
            Book book;

            try
            {
                book = _bookiContext.Books.Where(b => b.Id == bookId).FirstOrDefault();
            }catch(Exception ex) 
            {
                book = null;
            }

            return book;
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

        public Book UpdateBook(Book book)
        {
            Book updateddBook;

            try
            {
                book.CreationDate = _bookiContext.Books.Where(b => b.Id == book.Id).Select(b => b.CreationDate).FirstOrDefault();
                var result = _bookiContext.Update(book);
                updateddBook = result.Entity;

                Save();
            }
            catch (Exception ex)
            {
                updateddBook = null;
            }

            return updateddBook;
        }

        public bool CheckBookBelongsToUser(int bookId, int userId)
        {
            bool belongsToUser;

            try
            {
                var bookshelves = _bookiContext.Users.Where(u => u.Id == userId).Select(u => u.Bookshelf);
                belongsToUser = bookshelves.Select(b => b.Books.Select(ib => ib.Id).Contains(bookId)).Any();
            }catch (Exception ex)
            {
                belongsToUser = false;
            }

            return belongsToUser;
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
