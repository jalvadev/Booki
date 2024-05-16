using AutoMapper;
using Booki.Helpers;
using Booki.Models;
using Booki.Models.DTOs;
using Booki.Repositories.Interfaces;
using Booki.Wrappers;
using Booki.Wrappers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Booki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;
        public BooksController(IBookRepository bookRepository, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userIdResponse = JWTHelper.GetUserIdFromHttpContext(HttpContext);
            if (!userIdResponse.Success)
                return BadRequest(userIdResponse);

            int userId = (userIdResponse as ComplexResponse<int>).Result;

            var bookRepsonse = ListBooksByUser(userId);
            if (!bookRepsonse.Success)
                return BadRequest(bookRepsonse);

            return Ok(bookRepsonse);
        }

        [HttpGet("/{bookId}")]
        public IActionResult Detail(int bookId)
        {
            var userIdResponse = JWTHelper.GetUserIdFromHttpContext(HttpContext);
            if (!userIdResponse.Success)
                return BadRequest(userIdResponse);

            int userId = (userIdResponse as ComplexResponse<int>).Result;

            var bookBelongsToUserResponse = CheckIfBookBelongToUser(bookId, userId);
            if (!bookBelongsToUserResponse.Success)
                return BadRequest(bookBelongsToUserResponse);

            var bookDetailResponse = GetBookDetail(bookId);
            if (!bookDetailResponse.Success)
                return BadRequest(bookDetailResponse);

            return Ok(bookDetailResponse);
        }

        [HttpPost("[action]")]
        public IActionResult Insert(BookDTO newBook)
        {
            IResponse response = CheckMandatoryFields(newBook);
            if (!response.Success)
                return BadRequest(response);

            response = JWTHelper.GetUserFromHttpContext(HttpContext);
            if (!response.Success)
                return BadRequest(response);

            User user = (response as ComplexResponse<User>).Result;

            response = InsertBook(newBook, user);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("[action]")]
        public IActionResult Update(BookDTO book)
        {
            IResponse response = CheckMandatoryFields(book);
            if (!response.Success)
                return BadRequest(response);

            response = JWTHelper.GetUserIdFromHttpContext(HttpContext);
            if (!response.Success)
                return BadRequest(response);

            var responseJWT = response as ComplexResponse<int>;
            int userId = responseJWT.Result;

            response = CheckIfBookBelongToUser(book.Id, userId);
            if (!response.Success)
                return BadRequest(response);

            response = UpdateBook(book);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        #region Private Methods
        private IResponse ListBooksByUser(int userId)
        {
            IResponse response;

            List<Book> booksByUser = _bookRepository.GetBooksByUserId(userId);

            response = booksByUser == null ?
                new SimpleResponse { Success = false, Message = "Hubo un error al recuperar los libros." } :
                new ComplexResponse<List<Book>> { Success = true, Message = "Libros obtenidos correctamente.", Result = booksByUser };

            return response;
        }

        private IResponse CheckIfBookBelongToUser(int bookId, int userId)
        {
            IResponse response;

            bool belongsToUser = _bookRepository.CheckBookBelongsToUser(bookId, userId);

            response = !belongsToUser ? 
                new SimpleResponse { Success = false, Message = "El libro no pertenece al usuario." } : 
                new SimpleResponse { Success = true, Message = "EL libro pertenece al usuario." };

            return response;
        }

        private IResponse GetBookDetail(int bookId)
        {
            IResponse response;

            Book book = _bookRepository.GetBookDetail(bookId);

            response = book == null ? 
                new SimpleResponse { Success = false, Message = "No se pudo obtener el detalle del libro." } : 
                new ComplexResponse<Book> { Success = true, Message = "Detalle del libro obtenido correctamente.", Result = book };

            return response;
        }

        private IResponse CheckMandatoryFields(BookDTO newBook)
        {
            IResponse response;

            if (newBook == null)
                response = new SimpleResponse { Success = false, Message = "Faltan campos por rellenar." };
            else if (string.IsNullOrEmpty(newBook.CoverPicture))
                response = new SimpleResponse { Success = false, Message = "El libro debe tener una portada." };
            else if (string.IsNullOrEmpty(newBook.Title))
                response = new SimpleResponse { Success = false, Message = "El libro debe tener un título." };
            else
                response = new SimpleResponse { Success = true, Message = "Todos los campos ok." };

            return response;
        }

        private IResponse InsertBook(BookDTO newBook, User user)
        {
            IResponse response;
            Book bookToInsert;

            response = SaveCoverImage(newBook, user.Username);

            if (response.Success)
            {
                response = MapBookObject(newBook);
            }

            if (response.Success)
            {
                var bookResponse = response as ComplexResponse<Book>;
                bookToInsert = bookResponse.Result;
                bookToInsert = _bookRepository.InsertBook(bookToInsert, user.Id);

                response = bookToInsert == null ? 
                    new SimpleResponse { Success = false, Message = "No se ha podido insertar el libro." } : 
                    new ComplexResponse<Book> { Success = true, Message = "El libro se ha insertado correctamente.", Result = bookToInsert };
            }

            return response;
        }

        private IResponse SaveCoverImage(BookDTO newBook, string userName)
        {
            IResponse response;

            try
            {
                string userDirectoryPath = ImageHelper.CreateUserDirectoryIfNotExists(userName);
                string booksDirectoryPath = ImageHelper.CreateBooksDirectoryIfNotExists(userName);

                byte[] coverBytes = ImageHelper.ConvertBase64OnBytes(newBook.CoverPicture);

                string currentBookPath = $"{booksDirectoryPath}/{Guid.NewGuid()}.jpg";
                newBook.CoverPicture = currentBookPath;

                bool saved = ImageHelper.SaveImage(currentBookPath, coverBytes);

                response = new SimpleResponse { Success = saved, Message = "Libro guardado." };
            }
            catch (Exception e)
            {
                response = new SimpleResponse { Success = false, Message = "Error al guardar la imagen" };
            }

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

        private IResponse UpdateBook(BookDTO book)
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

        #endregion
    }
}
