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
using Ollama_DB_layer.Repositories.PromptRepo;
using Ollama_DB_layer.Repositories.AIResponseRepo;
using Ollama_DB_layer.Repositories.ConversationUserPromptRepo;
using Ollama_DB_layer.Repositories.ConversationRepo;
using Ollama_DB_layer.UOW;
using Ollama_Component.Services.ExploreService;
using Ollama_DB_layer.Repositories.FeedbackRepo;
using Ollama_DB_layer.Repositories.ModelTageRepo;
using Ollama_DB_layer.Repositories.PaginationRepo;
using Ollama_DB_layer.Repositories.SystemMessageRepo;
using Ollama_DB_layer.Repositories.TagRepo;
using Ollama_Component.Services.ConversationService;
using Ollama_DB_layer.Repositories.GetHistoryRepo;
using Ollama_DB_layer.Repositories.SetHistoryRepo;
using Ollama_Component.Services.CacheService;
using FluentValidation;
using Ollama_Component.Controllers;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ollama_Component.Services.AuthService.Helpers;
using Ollama_Component.Services.AuthService;
using Microsoft.AspNetCore.Identity;
using Ollama_DB_layer.Entities;


//here ia commit from linux
namespace Ollama_Component;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        builder.Services.AddDbContext<MyDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Identity Configuration
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
        })
          .AddEntityFrameworkStores<MyDbContext>()
          .AddDefaultTokenProviders(); // This ensures a token provider is registered



        // JWT Configuration
        builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT")); 

        // Register JWTManager
        builder.Services.AddScoped<JWTManager>(); 

        // Add AuthService 
        builder.Services.AddScoped<IAuthService, AuthService>();


        builder.Services.AddAuthentication(options =>
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
                ValidIssuer = builder.Configuration["JWT:Issuer"],
                ValidAudience = builder.Configuration["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
                ClockSkew=TimeSpan.Zero
                

            };
        });




        // Add repositories
        builder.Services.AddScoped<IAIModelRepository, AIModelRepository>();
        builder.Services.AddScoped<IAIResponseRepository, AIResponseRepository>();
        builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
        builder.Services.AddScoped<IConversationPromptResponseRepository, ConversationPromptResponseRepository>();
        builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
        builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        builder.Services.AddScoped<IModelTagRepository, ModelTagRepository>();
        builder.Services.AddScoped<IPaginationRepository, PaginationRepository>();
        builder.Services.AddScoped<IPromptRepository, PromptRepository>();
        builder.Services.AddScoped<ISystemMessageRepository, SystemMessageRepository>();
        builder.Services.AddScoped<ITagRepository, TagRepository>();
        builder.Services.AddScoped<IGetHistoryRepository, GetHistoryRepository>();
        builder.Services.AddScoped<ISetHistoryRepository, SetHistoryRepository>();
      
       

        // Register UnitOfWork
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add Ollama API client and Semantic Kernel configuration
        builder.Services.AddScoped<IOllamaApiClient>(_ => new OllamaApiClient("http://localhost:11434"));
        builder.Services.AddScoped<IOllamaConnector, OllamaConnector>();
        builder.Services.AddScoped<ChatHistory>();
        builder.Services.AddScoped<ChatHistoryManager>();
        builder.Services.AddScoped<IChatService, ChatService>();
        builder.Services.AddScoped<IAdminService, AdminService>();
        builder.Services.AddScoped<IConversationService, ConversationService>();
        builder.Services.AddScoped<IExploreService, ExploreService>();

        // Validators
        builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        builder.Services.AddMemoryCache();


        // Add Redis
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

        // Register CacheManager
        builder.Services.AddScoped<CacheManager>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5173") // Allow only this origin
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // If using cookies or authentication
            });
        });

        var app = builder.Build();

        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1"));
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        // ✅ Apply the CORS policy correctly
        app.UseCors("AllowFrontend");

        app.UseAuthorization();
        app.MapControllers();

        app.Run();


    }
}