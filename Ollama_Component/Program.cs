using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Ollama_Component.Connectors;
using Ollama_Component.Services.ChatService;
using OllamaSharp;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Ollama_DB_layer.Persistence;
using Ollama_DB_layer.Repositories.ApplicationUserRepo;

namespace Ollama_Component;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Add Ollama API client and Semantic Kernel configuration
        builder.Services.AddScoped<IOllamaApiClient>(_ => new OllamaApiClient("http://localhost:11434"));
        builder.Services.AddScoped<OllamaConnector>();
        builder.Services.AddScoped<ChatHistory>();
        builder.Services.AddScoped<ISemanticKernelService, SemanticKernelService>();
        builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();

        builder.Services.AddMemoryCache();


        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSwagger", policy =>
            {
                policy.WithOrigins("http://localhost:7006/swagger/index.html", "https://localhost:7006/swagger/index.html") // Replace with your Swagger domain
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // Use the CORS policy
        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1"));
            app.MapScalarApiReference();
            app.UseCors("AllowSwagger");

        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
