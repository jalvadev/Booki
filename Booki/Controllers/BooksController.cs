using AutoMapper;
using Booki.Helpers;
using Booki.Models;
using Booki.Models.DTOs;
using Booki.Wrappers;
using Booki.Wrappers.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Booki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IMapper _mapper;
        public BooksController(IMapper mapper)
        {
            _mapper = mapper;
        }

        [HttpPost]
        public IActionResult InsertBook(BookDTO newBook)
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

            response = MapBookObject(newBook);


            return response;
        }

        private IResponse MapBookObject(BookDTO book)
        {
            IResponse response;
            Book bookMapped = _mapper.Map<Book>(book);

            return response = new ComplexResponse<Book> { Success = true, Message = "Object mapped", Result = bookMapped };
        }
        #endregion
    }
}
