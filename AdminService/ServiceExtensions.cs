using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Reflection;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.Persistence;
using Ollama_DB_layer.Repositories.AIModelRepo;
using Ollama_DB_layer.Repositories.AIResponseRepo;
using Ollama_DB_layer.Repositories.ApplicationUserRepo;
using Ollama_DB_layer.Repositories.ConversationRepo;
using Ollama_DB_layer.Repositories.ConversationUserPromptRepo;
using Ollama_DB_layer.Repositories.FeedbackRepo;
using Ollama_DB_layer.Repositories.GetHistoryRepo;
using Ollama_DB_layer.Repositories.ModelTageRepo;
using Ollama_DB_layer.Repositories.PaginationRepo;
using Ollama_DB_layer.Repositories.PromptRepo;
using Ollama_DB_layer.Repositories.SetHistoryRepo;
using Ollama_DB_layer.Repositories.SystemMessageRepo;
using Ollama_DB_layer.Repositories.TagRepo;
using Ollama_DB_layer.Repositories.RefreshTokenRepo;
using Ollama_DB_layer.Repositories.AttachmentRepo;
using Ollama_DB_layer.UOW;
using OllamaSharp;
using StackExchange.Redis;
using FluentValidation;
using FluentValidation.AspNetCore;
using AdminService.Controllers;
using AdminService.Connectors;
using AdminService.Services.InferenceOperations;
using AdminService.Services.UserOperations;
using AdminService.Services.UserOperations.DTOs;
using AdminService.Controllers.Validators;
using AdminService.Services.AIModelOperations;
using AdminService.Services.AIModelOperations.DTOs;
using Ollama_DB_layer.Repositories.FolderRepo;
using Ollama_DB_layer.Repositories.NoteRepo;
using AdminService.Services.TagsOperations;

namespace AdminService
{
    public static class ServiceExtensions
    {
        // Register Database and Identity Services
        public static void AddDatabaseAndIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<MyDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
            })
            .AddEntityFrameworkStores<MyDbContext>()
            .AddDefaultTokenProviders();
        }

        // Register Authentication & Authorization
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


        // Register Repositories
        public static void AddRepositories(this IServiceCollection services)
        {

            services.AddScoped<IAIModelRepository, AIModelRepository>();
            services.AddScoped<IAIResponseRepository, AIResponseRepository>();
            services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
            services.AddScoped<IConversationPromptResponseRepository, ConversationPromptResponseRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            services.AddScoped<IModelTagRepository, ModelTagRepository>();
            services.AddScoped<IPaginationRepository, PaginationRepository>();
            services.AddScoped<IPromptRepository, PromptRepository>();
            services.AddScoped<ISystemMessageRepository, SystemMessageRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IGetHistoryRepository, GetHistoryRepository>();
            services.AddScoped<ISetHistoryRepository, SetHistoryRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IAttachmentRepository, AttachmentRepository>();
            services.AddScoped<IFolderRepository, FolderRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        // Register Services
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IOllamaApiClient>(_ => new OllamaApiClient("http://localhost:11434"));
            services.AddScoped<IOllamaConnector, OllamaConnector>();


            services.AddScoped<IInferenceOperationsService, InferenceOperationsService>();
            services.AddScoped<IUserOperationsService, UserOperationsService>();
            services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();


            services.AddScoped<ITagsOperationsService, TagsOperationsService>();

            services.AddScoped<IAIModelOperationsService, AIModelOperationsService>();
            services.AddScoped<IValidator<CreateModelRequest>, CreateModelRequestValidator>();
            services.AddScoped<IValidator<UpdateModelRequest>, UpdateModelRequestValidator>();
            services.AddScoped<IValidator<ModelTagOperationRequest>, ModelTagOperationRequestValidator>();
            services.AddScoped<IValidator<SearchModelRequest>, SearchModelRequestValidator>();

            // Add logging services if not already registered elsewhere
            services.AddLogging();
        }

        // Register CORS
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });
        }

        // Register Redis Cache
        public static void ConfigureCache(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));
            //services.AddScoped<CacheManager>();
        }

        // Register Swagger with JWT Support
        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AdminService", Version = "v1" });

                // ✅ Add JWT Authentication support in Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer {your token}' below:"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                    new OpenApiSecurityScheme
                        {
                        Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                    new string[] {}
                    }
                });
            });
        }

    }

}
