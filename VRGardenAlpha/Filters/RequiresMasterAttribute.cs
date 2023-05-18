using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using VRGardenAlpha.Models.Options;

namespace VRGardenAlpha.Filters
{
    [System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class RequiresMasterAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
        {
            var options = ctx.HttpContext.RequestServices.GetRequiredService<IOptions<GardenOptions>>();
            string? claim = ctx.HttpContext.Request.Headers["X-Master-Password"];

            if (options.Value.MasterPassword != claim)
                ctx.Result = new UnauthorizedObjectResult(new { error = "credentials.invalid" });
            else
                await next();
        }
    }
}
