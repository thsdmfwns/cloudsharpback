using Newtonsoft.Json;

namespace cloudsharpback.Models.DTO
{
    public class HttpResponseDto
    {
        public int HttpCode { get; set; }
        public string? Message { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
