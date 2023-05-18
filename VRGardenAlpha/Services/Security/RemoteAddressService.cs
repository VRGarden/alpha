using System.Net;

namespace VRGardenAlpha.Services.Security
{
    public class RemoteAddressService : IRemoteAddressService
    {
        public IPAddress GetIPAddress(HttpContext ctx)
        {
            var ip = ctx.Connection.RemoteIpAddress;
            string? header = ctx.Request.Headers["CF-Connecting-IP"];

            if(header != null)
                _ = IPAddress.TryParse(header, out ip);
            
            return ip ?? IPAddress.None;
        }
    }
}
