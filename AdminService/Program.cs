﻿using Scalar.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerUI; 
using Microsoft.AspNetCore.Builder;
namespace AdminService;

public class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        // Use Organized Service Registrations
        builder.Services.AddDatabaseAndIdentity(builder.Configuration);
        //builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddRepositories();
        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.ConfigureCors();
        builder.Services.ConfigureCache(builder.Configuration);
        builder.Services.ConfigureSwagger();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
        {
            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                options.DisplayRequestDuration();
            });

            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowFrontend");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();

    }
}