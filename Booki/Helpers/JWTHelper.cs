using Booki.Wrappers;
using Booki.Wrappers.Interfaces;

namespace Booki.Helpers
{
    public static class JWTHelper
    {
        public static IResponse GetUserIdFromHttpContext(HttpContext httpContext)
        {
            return new ComplexResponse<int> { Success = true, Message = "User ID obtained successfuly.", Result = 1 };
        }
    }
}
