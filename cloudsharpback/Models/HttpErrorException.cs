namespace cloudsharpback.Models
{
    public class HttpErrorException : Exception
    {
        public int ErrorCode { get; set; }
        public ServiceResult ErrorDetail => new ServiceResult()
        {
            ErrorCode = ErrorCode,
            Message = base.Message,
        };

        public HttpErrorException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public HttpErrorException(ServiceResult httpErrorDetail) : base(httpErrorDetail.Message)
        {
            ErrorCode = httpErrorDetail.ErrorCode;
        }

    }
}
