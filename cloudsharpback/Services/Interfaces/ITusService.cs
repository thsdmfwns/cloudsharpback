using cloudsharpback.Models;
using tusdotnet.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface ITusService
    {
        DefaultTusConfiguration GetTusConfiguration();
    }
}