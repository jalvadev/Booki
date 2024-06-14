using AutoMapper;
using Booki.Helpers;
using Booki.Models;
using Booki.Models.DTOs;
using Booki.Repositories.Interfaces;
using Booki.Services.Interfaces;
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
        private readonly IBookService _bookService;
        private readonly IFileService _imageService;
        public BooksController(IBookService bookService, IFileService imageService)
        {
            _bookService = bookService;
            _imageService = imageService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userIdResponse = JWTHelper.GetUserIdFromHttpContext(HttpContext);
            if (!userIdResponse.Success)
                return BadRequest(userIdResponse);

            int userId = (userIdResponse as ComplexResponse<int>).Result;

            var bookRepsonse = _bookService.ListBooksByUserId(userId);
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

            var bookBelongsToUserResponse = _bookService.CheckIfBookBelongToUser(bookId, userId);
            if (!bookBelongsToUserResponse.Success)
                return BadRequest(bookBelongsToUserResponse);

            var bookDetailResponse = _bookService.GetBookDetail(bookId);
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

            response = _bookService.CheckIfBookBelongToUser(book.Id, userId);
            if (!response.Success)
                return BadRequest(response);

            response = _bookService.UpdateBook(book);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("[action]")]
        public IActionResult Delete(int id)
        {
            IResponse response = JWTHelper.GetUserIdFromHttpContext(HttpContext);
            if (!response.Success)
                return BadRequest(response);

            var responseJWT = response as ComplexResponse<int>;
            int userId = responseJWT.Result;

            response = _bookService.DeleteBook(id, userId);
            if(!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        #region Private Methods

        private IResponse CheckMandatoryFields(BookDTO newBook)
        {
            IResponse response;

            if (newBook == null)
                response = new SimpleResponse { Success = false, Message = "Faltan campos por rellenar." };
            else if (string.IsNullOrEmpty(newBook.CoverPicture))
                response = new SimpleResponse { Success = false, Message = "El libro debe tener una portada." };
            else if (string.IsNullOrEmpty(newBook.Title))
                response = new SimpleResponse { Success = false, Message = "El libro debe tener un título." };
            else if (newBook.Rating <= 0 || newBook.Rating > 5)
                response = new SimpleResponse { Success = false, Message = "La puntuación es obligatoria, el mínimo es una estrella." };
            else if (!newBook.FinishDate.HasValue)
                response = new SimpleResponse { Success = false, Message = "La fecha es obligatoria." };
            else
                response = new SimpleResponse { Success = true, Message = "Todos los campos ok." };

            return response;
        }

        private IResponse InsertBook(BookDTO newBook, User user)
        {
            IResponse response;
            

            response = _imageService.SaveCoverImage(newBook, user.Username);

            if(response.Success)
                _bookService.InsertBook(newBook, user.Id);

            return response;
        }
        #endregion
    }
}
