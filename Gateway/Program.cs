using Gateway.Middlewares;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Gateway;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        //Add Ocelot Services
        builder.Configuration.AddJsonFile("ocelotConfig.json", optional: false, reloadOnChange: true);
        builder.Services.AddJwtAuthentication(builder.Configuration);

        builder.Services.AddOcelot();

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
