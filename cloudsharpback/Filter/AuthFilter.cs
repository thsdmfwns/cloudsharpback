using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace cloudsharpback.Filter
{
    public class AuthFilter : IAuthorizationFilter
    {
        private readonly IJWTService _jwtService;

        public AuthFilter(IJWTService jwtService)
        {
            _jwtService = jwtService;
        }

        private ObjectResult StatusCode(int statusCode, object value) => new ObjectResult(value) { StatusCode = statusCode };
        private StatusCodeResult StatusCode(int statusCode) => new StatusCodeResult(statusCode);

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var filters = context.Filters;
            var meta = context.HttpContext.GetEndpoint()!.Metadata;
            if (context.Filters.Any(x => x is IAllowAnonymousFilter)
                || context.HttpContext.GetEndpoint()!.Metadata.Any(x => x is AllowAnonymousAttribute))
            {
                return;
            }

            var auth = context.HttpContext.Request.Headers["auth"].FirstOrDefault();
            if (auth is null)
            {
                context.Result = StatusCode(401);
                return;
            }
            if (!_jwtService.TryValidateAcessToken(auth, out var memberDto)
                 || memberDto is null)
            {
                context.Result = StatusCode(403, "bad auth");
                return;
            }
            context.HttpContext.Items.Add("member", memberDto);
        }
    }
}
