using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Filter
{
    public class AuthFilter : IAuthorizationFilter
    {
        private readonly IJWTService jwtService;

        public AuthFilter(IJWTService jwtService)
        {
            this.jwtService = jwtService;
        }

        private ObjectResult StatusCode(int statusCode, object value) => new ObjectResult(value) { StatusCode = statusCode };

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var auth = context.HttpContext.Request.Headers["auth"].FirstOrDefault();
            if (auth is null)
            {
                context.Result = StatusCode(401, "No Auth");
                return;
            }

            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                 || memberDto is null)
            {
                context.Result = StatusCode(403, "bad auth");
                return;
            }

            context.HttpContext.Items.Add("member", memberDto);
        }
    }
}
