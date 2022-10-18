namespace cloudsharpback.Models
{
    public class HttpErrorException : Exception
    {
        public int ErrorCode { get; set; }
        public HttpErrorDetail ErrorDetail => new HttpErrorDetail()
        {
            ErrorCode = ErrorCode,
            Message = base.Message,
        };

        public HttpErrorException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public HttpErrorException(HttpErrorDetail httpErrorDetail) : base(httpErrorDetail.Message)
        {
            ErrorCode = httpErrorDetail.ErrorCode;
        }

    }
}
