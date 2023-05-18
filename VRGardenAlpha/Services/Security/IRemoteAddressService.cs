using System.Net;

namespace VRGardenAlpha.Services.Security
{
    public interface IRemoteAddressService
    {
        IPAddress GetIPAddress(HttpContext ctx);
    }
}
