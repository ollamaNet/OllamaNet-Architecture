using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Use Organized Service Registrations
        builder.Services.AddDatabaseAndIdentity(builder.Configuration);
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddRepositories();
        builder.Services.AddApplicationServices();
        builder.Services.ConfigureCors();
        builder.Services.ConfigureCache(builder.Configuration);
        builder.Services.ConfigureSwagger();
        builder.Services.AddControllers();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Seed roles
        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roles = { "User", "Admin" }; // Add other roles as needed

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
        {
            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
                options.DisplayRequestDuration();
            });

            //app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowFrontend");

        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
