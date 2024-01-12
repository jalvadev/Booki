namespace Booki.Wrappers.Interfaces
{
    public interface IResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; }
    }
}
