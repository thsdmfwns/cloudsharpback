using cloudsharpback.Attribute;
using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;
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
                    throw new HttpErrorException(new HttpResponseDto
                    {
                        HttpCode = 500,
                        Message = "Can not found memberDto in HttpContext"
                    });
                }
                return member;
            } 
        }
    }
}
