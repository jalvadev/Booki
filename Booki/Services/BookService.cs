using AutoMapper;
using Booki.Models;
using Booki.Models.DTOs;
using Booki.Repositories.Interfaces;
using Booki.Services.Interfaces;
using Booki.Wrappers.Interfaces;
using Booki.Wrappers;

namespace Booki.Services
{
    public class BookService : IBookService
    {
        private IMapper _mapper;
        private IBookRepository _bookRepository;

        public BookService(IMapper mapper, IBookRepository repository) 
        {
            _mapper = mapper;
            _bookRepository = repository;
        }

        public IResponse InsertBook(BookDTO book, int userId)
        {
            IResponse response;
            Book bookToInsert;

            response = MapBookObject(book);

            if (response.Success)
            {
                var bookResponse = response as ComplexResponse<Book>;
                bookToInsert = bookResponse.Result;
                bookToInsert = _bookRepository.InsertBook(bookToInsert, userId);

                response = bookToInsert == null ?
                    new SimpleResponse { Success = false, Message = "No se ha podido insertar el libro." } :
                    new ComplexResponse<Book> { Success = true, Message = "El libro se ha insertado correctamente.", Result = bookToInsert };
            }

            return response;
        }

        public IResponse GetBookDetail(int bookId)
        {
            IResponse response;

            Book book = _bookRepository.GetBookDetail(bookId);

            response = book == null ?
                new SimpleResponse { Success = false, Message = "No se pudo obtener el detalle del libro." } :
                new ComplexResponse<Book> { Success = true, Message = "Detalle del libro obtenido correctamente.", Result = book };

            return response;
        }

        public IResponse ListBooksByUserId(int userId)
        {
            IResponse response;

            List<Book> booksByUser = _bookRepository.GetBooksByUserId(userId);

            response = booksByUser == null ?
                new SimpleResponse { Success = false, Message = "Hubo un error al recuperar los libros." } :
                new ComplexResponse<List<Book>> { Success = true, Message = "Libros obtenidos correctamente.", Result = booksByUser };

            return response;
        }

        public IResponse UpdateBook(BookDTO book)
        {
            IResponse response;
            Book bookToUpdate;

            response = MapUpdateBookObject(book);

            if (response.Success)
            {
                var bookResponse = response as ComplexResponse<Book>;
                bookToUpdate = bookResponse.Result;
                bookToUpdate = _bookRepository.UpdateBook(bookToUpdate);

                response = bookToUpdate == null ?
                    new SimpleResponse { Success = false, Message = "No se ha podido actualizar el libro." } :
                    new ComplexResponse<Book> { Success = true, Message = "El libro se ha actualizado correctamente.", Result = bookToUpdate };
            }

            return response;
        }

        public IResponse CheckIfBookBelongToUser(int bookId, int userId)
        {
            IResponse response;

            bool belongsToUser = _bookRepository.CheckBookBelongsToUser(bookId, userId);

            response = !belongsToUser ?
                new SimpleResponse { Success = false, Message = "El libro no pertenece al usuario." } :
                new SimpleResponse { Success = true, Message = "EL libro pertenece al usuario." };

            return response;
        }

        private IResponse MapBookObject(BookDTO book)
        {
            IResponse response;

            try
            {
                Book bookMapped = _mapper.Map<Book>(book);

                bookMapped.CreationDate = DateTime.Now;
                bookMapped.LastUpdate = DateTime.Now;

                response = new ComplexResponse<Book> { Success = true, Message = "Object mapped", Result = bookMapped };
            }
            catch (Exception ex)
            {
                response = new SimpleResponse { Success = false, Message = ex.Message };
            }


            return response;
        }

        private IResponse MapUpdateBookObject(BookDTO book)
        {
            IResponse response;

            try
            {
                Book bookMapped = _mapper.Map<Book>(book);
                bookMapped.LastUpdate = DateTime.Now;

                response = new ComplexResponse<Book> { Success = true, Message = "Object mapped", Result = bookMapped };
            }
            catch (Exception ex)
            {
                response = new SimpleResponse { Success = false, Message = ex.Message };
            }


            return response;
        }

    }
}
