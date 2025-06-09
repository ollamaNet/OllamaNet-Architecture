using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Gateway.Middlewares
{
    public class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleAuthorizationMiddleware> _logger;
        private readonly Dictionary<string, string[]> _endpointRoleMap;

        public RoleAuthorizationMiddleware(RequestDelegate next, ILogger<RoleAuthorizationMiddleware> logger, Dictionary<string, string[]> endpointRoleMap)
        {
            _next = next;
            _logger = logger;
            _endpointRoleMap = endpointRoleMap;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
            var method = context.Request.Method;

            // Find the most specific matching pattern (longest prefix match)
            var matched = _endpointRoleMap
                .OrderByDescending(kv => kv.Key.Length)
                .FirstOrDefault(kv => path.StartsWith(kv.Key.ToLower()));

            if (!string.IsNullOrEmpty(matched.Key) && matched.Value.Length > 0)
            {
                if (!context.User.Identity?.IsAuthenticated ?? true)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }
                var userRoles = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
                if (!matched.Value.Any(role => userRoles.Contains(role)))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Forbidden: Insufficient role");
                    return;
                }
            }
            await _next(context);
        }
    }
} 