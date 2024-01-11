using Booki.Wrappers.Interfaces;

namespace Booki.Wrappers
{
    public class SimpleResponse : IResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
