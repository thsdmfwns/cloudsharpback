using cloudsharpback.Attribute;
using cloudsharpback.Models;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers.Base
{
    [Auth]
    public class AuthControllerBase : ControllerBase
    {
        public MemberDto? Member => HttpContext.Items["member"] as MemberDto;
    }
}
