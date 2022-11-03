namespace cloudsharpback.Models
{
    public class HttpErrorException : Exception
    {
        public int ErrorCode { get; set; }
        public HttpErrorDto ErrorDetail => new HttpErrorDto()
        {
            ErrorCode = ErrorCode,
            Message = base.Message,
        };

        public HttpErrorException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public HttpErrorException(HttpErrorDto httpErrorDetail) : base(httpErrorDetail.Message)
        {
            ErrorCode = httpErrorDetail.ErrorCode;
        }

    }
}
