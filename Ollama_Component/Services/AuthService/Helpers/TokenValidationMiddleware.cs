using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ollama_DB_layer.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ollama_Component.Services.AuthService.Helpers
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly JWT _jwt;

        public TokenValidationMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory, IOptions<JWT> jwt)
        {
            _next = next;
            _scopeFactory = scopeFactory;
            _tokenHandler = new JwtSecurityTokenHandler();
            _jwt = jwt.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = ExtractTokenFromRequest(context);

            if (!string.IsNullOrEmpty(token))
            {
                var principal = ValidateJwtToken(token);

                if (principal != null)
                {
                    var userId = principal.FindFirst("UserId")?.Value;
                    var tokenVersionFromJwt = int.Parse(principal.FindFirst("token_version")?.Value ?? "0");

                   
                    using var scope = _scopeFactory.CreateScope();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    var user = await userManager.FindByIdAsync(userId);

                    if (user == null || user.TokenVersion > tokenVersionFromJwt)
                    {
                        
                        if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsync("Token has been invalidated");
                        }
                        return;
                    }


                }
            }
            
            await _next(context);
        }

        private string ExtractTokenFromRequest(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            return authHeader?.StartsWith("Bearer ") == true ? authHeader.Substring(7) : null;
        }

        private ClaimsPrincipal ValidateJwtToken(string token)
        {
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwt.Issuer,
                    ValidAudience = _jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key))
                };

                return _tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch
            {
                return null;
            }
        }
    }




}
