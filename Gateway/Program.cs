using Gateway.Middlewares;
using Gateway.Services.ConfigurationLoader;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System;

namespace Gateway;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add JWT authentication and CORS
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.ConfigureCors();

        // Use the new configuration loader instead of direct JSON file
        builder.Services.AddOcelotWithSplitConfigurations(builder.Configuration);

        // Add configuration change monitor
        builder.Services.AddSingleton<Action>(sp => 
        {
            // This action will be called when configuration changes
            return () => 
            {
                var newConfig = ConfigurationLoader.LoadAndCombineConfigurations();
                // In a production environment, you would use a more sophisticated mechanism
                // to update Ocelot's configuration at runtime
                Console.WriteLine("Configuration reloaded, restart application to apply changes.");
            };
        });
        builder.Services.AddHostedService<ConfigurationChangeMonitor>();





        // Add services to the container.
        builder.Services.AddControllers();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
        {
            app.MapOpenApi();

            //app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                options.DisplayRequestDuration();
            });

            //app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowFrontend");

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.UseMiddleware<ClaimsToHeaderMiddleware>();

        //Add Ocelot Middleware
        await app.UseOcelot();

        app.Run();
    }
}
