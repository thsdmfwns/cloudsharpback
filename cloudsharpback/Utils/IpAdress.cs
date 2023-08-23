namespace cloudsharpback.Utils;

public class IpAdressUtil
{
    public static string? Get(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(ipAddress) && ipAddress.Contains(":"))
        {
            ipAddress = ipAddress.Substring(0, ipAddress.IndexOf(":", StringComparison.Ordinal));
        }
        return ipAddress;
    }
}