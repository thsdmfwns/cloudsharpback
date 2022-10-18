using cloudsharpback.Models;

namespace cloudsharpback.Middleware
{
    public class HttpErrorMiddleware
    {
        private readonly RequestDelegate _next;
        public HttpErrorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (HttpErrorException ex)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                response.StatusCode = ex.ErrorCode;
                await response.WriteAsync(ex.ErrorDetail.ToString());
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
