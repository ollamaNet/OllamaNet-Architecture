using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Ollama_Component.Connectors;
using OllamaSharp;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Ollama_DB_layer.Persistence;
using Ollama_Component.Services.AdminServices;
using Ollama_Component.Services.ChatService;
using Ollama_DB_layer.Repositories.AIModelRepo;
using Ollama_DB_layer.Repositories.ApplicationUserRepo;
using Ollama_DB_layer.Repositories;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.Repositories.PromptRepo;
using Ollama_DB_layer.Repositories.AIResponseRepo;
using Ollama_DB_layer.Repositories.ConversationUserPromptRepo;
using Ollama_DB_layer.Repositories.ConversationRepo;

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


        // Add repositories
        builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
        builder.Services.AddScoped<IPromptRepository, PromptRepository>();
        builder.Services.AddScoped<IAIResponseRepository, AIResponseRepository>();
        builder.Services.AddScoped<IConversationPromptResponseRepository, ConversationPromptResponseRepository>();
        builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
        builder.Services.AddScoped<IAIModelRepository, AIModelRepository>();

        // Add Ollama API client and Semantic Kernel configuration
        builder.Services.AddScoped<IOllamaApiClient>(_ => new OllamaApiClient("http://localhost:11434"));
        builder.Services.AddScoped<IOllamaConnector, OllamaConnector>();
        builder.Services.AddScoped<ChatHistory>();
        builder.Services.AddScoped<ChatHistoryManager>();
        builder.Services.AddScoped<ChatCacheManager>();
        builder.Services.AddScoped<IChatService, ChatService>();
        builder.Services.AddScoped<IAdminService, AdminService>();


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
