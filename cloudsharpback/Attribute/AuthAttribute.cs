using cloudsharpback.Filter;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Attribute
{
    public class AuthAttribute : TypeFilterAttribute
    {
        public AuthAttribute() : base(typeof(AuthFilter))
        {
        }
    }
}
