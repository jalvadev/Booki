using AutoMapper;
using Booki.Helpers;
using Booki.Models;
using Booki.Models.DTOs;
using Booki.Repositories.Interfaces;
using Booki.Wrappers;
using Booki.Wrappers.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Booki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            IResponse response = JWTHelper.GetUserIdFromHttpContext(HttpContext);
            if (!response.Success)
                return BadRequest(response);

            var responseJWT = response as ComplexResponse<int>;
            int userId = responseJWT.Result;

            response = ListBooksByUser(userId);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        #region Private Index Methods
        
        private IResponse ListBooksByUser(int userId)
        {
            IResponse response;

            List<Book> booksByUser = _bookRepository.GetBooksByUserId(userId);
            
            response = booksByUser == null ? new SimpleResponse { Success = false, Message = "Hubo un error al recuperar los libros." } 
                : new ComplexResponse<List<Book>> { Success = true, Message = "Libros obtenidos correctamente.", Result = booksByUser };

            return response;
        }

        #endregion

        [HttpGet("/{bookId}")]
        public IActionResult Detail(int bookId)
        {
            IResponse response;

            response = GetBookDetail(bookId);
            if(!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        #region Private Detail Methods

        private IResponse GetBookDetail(int bookId)
        {
            IResponse response;

            Book book = _bookRepository.GetBookDetail(bookId);

            response = book == null ? new SimpleResponse { Success = false, Message = "No se pudo obtener el detalle del libro." }
                : new ComplexResponse<Book> { Success = true, Message = "Detalle del libro obtenido correctamente.", Result = book };

            return response;
        }

        #endregion

        [HttpPost]
        public IActionResult Insert(BookDTO newBook)
        {
            IResponse response = CheckMandatoryFields(newBook);
            if (!response.Success)
                return BadRequest(response);

            response = JWTHelper.GetUserIdFromHttpContext(HttpContext);
            if(!response.Success)
                return BadRequest(response);

            var responseJWT = response as ComplexResponse<int>;
            int userId = responseJWT.Result;

            response = InsertBook(newBook,userId);
            if(!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        #region Private Methods Insert

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

        private IResponse InsertBook(BookDTO newBook, int userId)
        {
            IResponse response;
            Book bookToInsert;

            response = MapBookObject(newBook);

            if (response.Success)
            {
                var bookResponse = response as ComplexResponse<Book>;
                bookToInsert =  bookResponse.Result;
                bookToInsert = _bookRepository.InsertBook(bookToInsert, userId);

                response = bookToInsert == null ? new SimpleResponse { Success = false, Message = "No se ha podido insertar el libro." } 
                    : new ComplexResponse<Book> { Success = true, Message = "El libro se ha insertado correctamente.", Result =  bookToInsert };
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
            catch(Exception ex) 
            {
                response = new SimpleResponse { Success = false, Message= ex.Message };
            }


            return response;
        }
        #endregion

        [HttpPut]
        public IActionResult Update(BookDTO book)
        {
            IResponse response = CheckMandatoryFields(book);
            if (!response.Success)
                return BadRequest(response);

            response = UpdateBook(book);
            if(!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        #region Private Methods Update

        private IResponse UpdateBook(BookDTO book)
        {
            IResponse response;
            Book bookToUpdate;

            response = MapBookObject(book);

            if (response.Success)
            {
                var bookResponse = response as ComplexResponse<Book>;
                bookToUpdate = bookResponse.Result;
                bookToUpdate = _bookRepository.UpdateBook(bookToUpdate);

                response = bookToUpdate == null ? new SimpleResponse { Success = false, Message = "No se ha podido actualizar el libro." }
                    : new ComplexResponse<Book> { Success = true, Message = "El libro se ha actualizado correctamente.", Result = bookToUpdate };
            }

            return response;
        }
        #endregion
    }
}
