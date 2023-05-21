using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using VRGardenAlpha.Models.Options;
using VRGardenAlpha.Services.Security;

namespace VRGardenAlpha.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RequiresCaptchaAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
        {
            var captcha = ctx.HttpContext.RequestServices.GetRequiredService<ICaptchaService>();
            var options = ctx.HttpContext.RequestServices.GetRequiredService<IOptions<GardenOptions>>();
            string? masterPwClaim = ctx.HttpContext.Request.Headers["X-Master-Password"];
            string? botPwClaim = ctx.HttpContext.Request.Headers["X-Bot-Password"];
            string? claim = ctx.HttpContext.Request.Headers["X-Captcha"];
            // Later, implement a thing that ignores captcha requirement for System bots and etc.

            if (!options.Value.RequiresCaptcha 
                || masterPwClaim == options.Value.MasterPassword
                || botPwClaim == options.Value.BotPassword)
            {
                await next();
                return;
            }

            if (!string.IsNullOrWhiteSpace(claim))
            {
                if (await captcha.VerifyCaptchaAsync(claim, ctx.HttpContext))
                {
                    await next();
                    return;
                }
            }

            ctx.Result = new BadRequestObjectResult(new { error = "captcha.invalid" });
        }
    }
}
