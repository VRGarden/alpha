namespace VRGardenAlpha.Services.Security
{
    public interface ICaptchaService
    {
        Task<bool> VerifyCaptchaAsync(string code, HttpContext? ctx = null);
    }
}
