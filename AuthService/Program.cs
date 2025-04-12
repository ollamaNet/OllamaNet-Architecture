using Microsoft.AspNetCore.Builder;

namespace AuthenticationService;

public class Program
{
    public static void Main(string[] args)
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
