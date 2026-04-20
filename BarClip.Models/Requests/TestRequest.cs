using Microsoft.AspNetCore.Http;

namespace BarClip.Models.Requests
{
    public class TestRequest
    {
        public IFormFile File { get; set; }
    }
}
