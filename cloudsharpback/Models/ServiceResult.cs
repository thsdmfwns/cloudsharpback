using Newtonsoft.Json;

namespace cloudsharpback.Models
{
    public class ServiceResult
    {
        public int ErrorCode { get; set; }
        public string? Message { get; set; }

        public bool IsSuccess => ErrorCode - 200 < 100;

        public static ServiceResult Success => new ServiceResult() { ErrorCode = 200 };
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
