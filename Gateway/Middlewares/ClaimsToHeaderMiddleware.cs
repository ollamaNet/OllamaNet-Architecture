using System.Security.Claims;

namespace Gateway.Middlewares
{
    public class ClaimsToHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public ClaimsToHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var claims = context.User.Claims.ToList();

                if (context.User.FindFirst("UserId") is { } userId)
                    context.Request.Headers["X-User-Id"] = userId.Value;

                if (context.User.FindFirst(ClaimTypes.Email) is { } email)
                    context.Request.Headers["X-User-Email"] = email.Value;

                if (context.User.FindFirst(ClaimTypes.Role) is { } role)
                    context.Request.Headers["X-User-Role"] = role.Value;
            }

            await _next(context);
        }
    }
}
