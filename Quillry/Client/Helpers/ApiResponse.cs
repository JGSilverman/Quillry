using System.Net;

namespace Quillry.Client.Helpers
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
    }
}
