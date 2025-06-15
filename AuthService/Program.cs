using AuthService.DataSeeding.Interfaces;
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
        builder.Services.AddEmailService(builder.Configuration);
        builder.Services.ConfigureCors();
        builder.Services.ConfigureCache(builder.Configuration);
        builder.Services.ConfigureSwagger();
        builder.Services.AddControllers();
        builder.Services.AddDataSeeding(builder.Configuration);

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Seed roles and admin user using new DataSeeder
        using (var scope = app.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
            if (await seeder.ShouldSeedAsync())
            {
                await seeder.SeedAsync();
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
