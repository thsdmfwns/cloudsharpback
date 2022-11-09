using cloudsharpback.Models;
using tusdotnet.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface ITusService
    {
        DefaultTusConfiguration GetTusConfiguration();
        HttpErrorDto? GetTusToken(MemberDto member, out Guid token);
    }
}