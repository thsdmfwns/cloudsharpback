namespace cloudsharpback.Models
{
    public class HttpErrorException : Exception
    {
        public int ErrorCode { get; set; }
        public HttpResponseDto ResponseDetail => new HttpResponseDto()
        {
            HttpCode = ErrorCode,
            Message = base.Message,
        };

        public HttpErrorException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public HttpErrorException(HttpResponseDto httpResponseDetail) : base(httpResponseDetail.Message)
        {
            ErrorCode = httpResponseDetail.HttpCode;
        }

    }
}
