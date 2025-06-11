using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Gateway.Configurations;

namespace Gateway
{
    public static class ServiceExtenstions
    {
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(o =>
             {
                 o.RequireHttpsMetadata = false;
                 o.SaveToken = false;
                 o.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuerSigningKey = true,
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidateLifetime = true,
                     ValidIssuer = configuration["JWT:Issuer"],
                     ValidAudience = configuration["JWT:Audience"],
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"])),
                 };
             });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
                options.AddPolicy("User", policy => policy.RequireRole("User"));
            });
        }

        // Register CORS
        public static void ConfigureCors(this IServiceCollection services, IConfiguration configuration)
        {
            var corsSettings = configuration.GetSection("CorsSettings").Get<CorsSettings>();
            
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    if (corsSettings?.AllowAllOrigins ?? false)
                    {
                        policy.AllowAnyOrigin();
                    }
                    else if (corsSettings?.AllowedOrigins?.Length > 0)
                    {
                        policy.WithOrigins(corsSettings.AllowedOrigins);
                        
                        if (corsSettings.AllowCredentials)
                        {
                            policy.AllowCredentials();
                        }
                    }
                    else
                    {
                        // Fallback to localhost if no origins are specified
                        policy.WithOrigins("http://localhost:5173")
                              .AllowCredentials();
                    }

                    if (corsSettings?.AllowedHeaders?.Length > 0)
                    {
                        policy.WithHeaders(corsSettings.AllowedHeaders);
                    }
                    else
                    {
                        policy.AllowAnyHeader();
                    }

                    if (corsSettings?.AllowedMethods?.Length > 0)
                    {
                        policy.WithMethods(corsSettings.AllowedMethods);
                    }
                    else
                    {
                        policy.AllowAnyMethod();
                    }
                });
            });
        }

        // Register Role-based Authorization Middleware
        public static void AddGatewayAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the role map as a singleton (loaded from RoleAuthorization.json)
            var configDir = Path.Combine(Directory.GetCurrentDirectory(), "Configurations");
            var roleAuthPath = Path.Combine(configDir, "RoleAuthorization.json");
            Dictionary<string, string[]> roleMap = new();
            if (File.Exists(roleAuthPath))
            {
                var json = File.ReadAllText(roleAuthPath);
                roleMap = JsonSerializer.Deserialize<Dictionary<string, string[]>>(json) ?? new();
            }
            services.AddSingleton(roleMap);
        }
    }
}
