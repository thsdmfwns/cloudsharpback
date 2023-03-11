using cloudsharpback.Attribute;
using cloudsharpback.Models;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers.Base
{
    [Auth]
    public class AuthControllerBase : ControllerBase
    {
        public MemberDto Member {
            get
            {
                var member = HttpContext.Items["member"] as MemberDto;
                if (member is null)
                {
                    throw new HttpErrorException(new HttpErrorDto
                    {
                        ErrorCode = 500,
                        Message = "Can not found memberDto in HttpContext"
                    });
                }
                return member;
            } 
        }
    }
}
