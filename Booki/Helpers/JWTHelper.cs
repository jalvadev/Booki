using Booki.Models;
using Booki.Wrappers;
using Booki.Wrappers.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Booki.Helpers
{
    public static class JWTHelper
    {
        public static string GenerateToken(User user, string key, string issuer, string audience)
        {
            var jwtKey = Encoding.UTF8.GetBytes(key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Name, user.Username),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(jwtKey), SecurityAlgorithms.HmacSha512Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var stringToken = tokenHandler.WriteToken(token);

            return stringToken;
        }

        public static IResponse GetUserIdFromHttpContext(HttpContext httpContext)
        {
            int userId;

            try
            {
                var response = GetUserFieldsFromHttpContextByName(httpContext, "Id");
                var resultId = (response as ComplexResponse<string>).Result;
                userId = Convert.ToInt32(resultId);

                return new ComplexResponse<int> { Success = true, Message = "User ID obtained successfuly.", Result = userId };
            }
            catch (Exception ex)
            {
                userId = -1;
                return new ComplexResponse<int> { Success = false, Message = "No se puede obtener la sesión del usuario.", Result = userId };
            }
        }

        public static IResponse GetUserNameFromHttpContext(HttpContext httpContext)
        {
            string userName;

            try
            {
                var response = GetUserFieldsFromHttpContextByName(httpContext, "Name");
                userName = (response as ComplexResponse<string>).Result;

                return new ComplexResponse<string> { Success = true, Message = "User ID obtained successfuly.", Result = userName };
            }
            catch (Exception ex)
            {
                userName = null;
                return new ComplexResponse<string> { Success = false, Message = "No se puede obtener la sesión del usuario.", Result = userName };
            }
        }


        public static IResponse GetUserFromHttpContext(HttpContext httpContext)
        {
            User user;

            try
            {
                var responseId = GetUserIdFromHttpContext(httpContext);
                if (!responseId.Success)
                    return responseId;

                var responseName = GetUserNameFromHttpContext(httpContext);
                if(!responseName.Success)
                    return responseName;

                user = new User
                {
                    Id = (responseId as ComplexResponse<int>).Result,
                    Username = (responseName as ComplexResponse<string>).Result
                };

                return new ComplexResponse<User> { Success = true, Message = "Usuario obtenido.", Result = user };
            }
            catch (Exception)
            {
                return new SimpleResponse { Success = false, Message = "No se pudo obtener el usuario." };
            }

        }

        private static IResponse GetUserFieldsFromHttpContextByName(HttpContext httpContext, string fieldName)
        {
            string userField;

            try
            {
                var user = httpContext.User;
                userField = user.FindFirstValue(fieldName);

                return new ComplexResponse<string> { Success = true, Message = "No se pudo obtener el campo del usuario.", Result = userField };
            }
            catch (Exception ex)
            {
                userField = null;
                return new ComplexResponse<string> { Success = false, Message = "No se puede obtener la sesión del usuario.", Result = userField };
            }
        }
    }
}
