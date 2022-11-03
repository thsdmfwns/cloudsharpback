using Newtonsoft.Json;

namespace cloudsharpback.Models
{
    public class HttpErrorDto
    {
        public int ErrorCode { get; set; }
        public string? Message { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
