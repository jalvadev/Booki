using Booki.Helpers;
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
        public IActionResult InsertBook(BookDTO newBook)
        {
            IResponse response = CheckMandatoryFields(newBook);
            if (!response.Success)
                return BadRequest(response);

            response = JWTHelper.GetUserIdFromHttpContext(HttpContext);
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
        #endregion
    }
}
