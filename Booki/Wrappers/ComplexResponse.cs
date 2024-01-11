using Booki.Wrappers.Interfaces;

namespace Booki.Wrappers
{
    public class ComplexResponse<T> : IResponse
    {
        public T Result { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
