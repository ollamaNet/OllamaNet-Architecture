using ConversationService.Cache;
using ConversationService.ChatService;
using ConversationService.ChatService.DTOs;
using ConversationService.Connectors;
using ConversationService.ConversationService;
using ConversationService.ConversationService.DTOs;
using ConversationService.Controllers.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel.ChatCompletion;
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
using Ollama_DB_layer.Repositories.RefreshTokenRepo;
using Ollama_DB_layer.Repositories.SetHistoryRepo;
using Ollama_DB_layer.Repositories.SystemMessageRepo;
using Ollama_DB_layer.Repositories.TagRepo;
using Ollama_DB_layer.UOW;
using OllamaSharp;
using StackExchange.Redis;
using System.Text;
using Ollama_DB_layer.Repositories.AttachmentRepo;
using Ollama_DB_layer.Repositories.FolderRepo;
using Ollama_DB_layer.Repositories.NoteRepo;
using ConversationService.FolderService.DTOs;
using ConversationService.FolderService;
using ConversationService.NoteService;
using ConversationService.FeedbackService;
using ConversationService.FeedbackService.DTOs;


namespace ConversationService
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
            services.AddScoped<IFolderRepository, FolderRepository>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            services.AddScoped<IModelTagRepository, ModelTagRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<IPaginationRepository, PaginationRepository>();
            services.AddScoped<IPromptRepository, PromptRepository>();
            services.AddScoped<ISystemMessageRepository, SystemMessageRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IGetHistoryRepository, GetHistoryRepository>();
            services.AddScoped<ISetHistoryRepository, SetHistoryRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            services.AddScoped<IAttachmentRepository, AttachmentRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        // Register Services
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IOllamaApiClient>(_ => new OllamaApiClient("https://704e-35-196-162-195.ngrok-free.app"));
            services.AddScoped<IOllamaConnector, OllamaConnector>();

            // Chat-related services
            services.AddScoped<ChatHistoryManager>();
            services.AddScoped<IChatService, ChatService.ChatService>();

            // folder service 
            services.AddScoped<IFolderService, FolderService.FolderService>();
            services.AddScoped<IValidator<CreateFolderRequest>, CreateFolderRequestValidator>();
            services.AddScoped<IValidator<UpdateFolderRequest>, UpdateFolderRequestValidator>();

            // Register ConversationService
            services.AddScoped<IConversationService, ConversationService.ConversationService>();

            // Register NoteService
            services.AddScoped<INoteService, NoteService.NoteService>();

            // Register FeedbackService
            services.AddScoped<IFeedbackService, FeedbackService.FeedbackService>();
            services.AddScoped<IValidator<AddFeedbackRequest>, AddFeedbackRequestValidator>();
            services.AddScoped<IValidator<UpdateFeedbackRequest>, UpdateFeedbackRequestValidator>();

            // Register validators from the new location
            services.AddScoped<IValidator<OpenConversationRequest>, Controllers.Validators.OpenConversationRequestValidator>();
            services.AddScoped<IValidator<UpdateConversationRequest>, Controllers.Validators.UpdateConversationRequestValidator>();

            // Register PromptRequestValidator (keep for backward compatibility)
            services.AddScoped<Controllers.Validators.PromptRequestValidator>();

            // Register ChatRequestValidator for the ChatController
            services.AddScoped<Controllers.Validators.ChatRequestValidator>();
            services.AddScoped<IValidator<PromptRequest>>(provider =>
            {
                // Use ChatRequestValidator for the ChatController
                return provider.GetRequiredService<Controllers.Validators.ChatRequestValidator>();
            });

            // Register HTTP context accessor
            services.AddHttpContextAccessor();
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
            // Register Redis cache settings from configuration
            services.Configure<RedisCacheSettings>(configuration.GetSection("RedisCacheSettings"));
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));

            // Register cache services with proper lifecycles
            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            services.AddSingleton<ICacheManager, CacheManager>();
        }

        // Register Swagger with JWT Support
        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ConversationService", Version = "v1" });

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
