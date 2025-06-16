using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Linq;
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
using AdminService.Services.InferenceOperations;
using AdminService.Services.UserOperations;
using AdminService.Services.UserOperations.DTOs;
using AdminService.Controllers.Validators;
using AdminService.Services.AIModelOperations;
using AdminService.Services.AIModelOperations.DTOs;
using Ollama_DB_layer.Repositories.FolderRepo;
using Ollama_DB_layer.Repositories.NoteRepo;
using AdminService.Services.TagsOperations;
using AdminService.Infrastructure.Extensions;
using AdminService.Infrastructure.Integration.InferenceEngine;
using AdminService.Infrastructure.Messaging.Consumers;
using AdminService.Infrastructure.Configuration;
using AdminService.Infrastructure.Messaging.Resilience;

namespace AdminService
{
    /// <summary>
    /// Extension methods for configuring and registering services
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Registers all application services
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The application configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register infrastructure services
            services.AddCaching(configuration);
            services.AddConfiguration(configuration);
            services.AddMessaging(configuration);

            // Register domain services
            services.AddInferenceServices(configuration);
            services.AddUserServices();
            services.AddModelServices();
            
            // Register health monitoring
            services.AddHealthMonitoring();

            // Register validators
            services.AddValidators();

            // Register logging
            services.AddLogging();

            return services;
        }

        /// <summary>
        /// Registers inference engine related services
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The application configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddInferenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add base OllamaApiClient for backward compatibility
            services.AddScoped<IOllamaApiClient>(_ => new OllamaApiClient("http://localhost:11434"));
            
            // Add the new inference engine connector
            services.AddScoped<IInferenceEngineConnector, InferenceEngineConnector>();
            
            // Add inference operations service
            services.AddScoped<IInferenceOperationsService, InferenceOperationsService>();
            
            return services;
        }

        /// <summary>
        /// Registers user related services
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddUserServices(this IServiceCollection services)
        {
            services.AddScoped<IUserOperationsService, UserOperationsService>();
            return services;
        }

        /// <summary>
        /// Registers model and tag related services
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddModelServices(this IServiceCollection services)
        {
            services.AddScoped<IAIModelOperationsService, AIModelOperationsService>();
            services.AddScoped<ITagsOperationsService, TagsOperationsService>();
            return services;
        }

        /// <summary>
        /// Registers validators for domain models
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            // Add FluentValidation validators
            services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
            
            // Register specific validators
            services.AddScoped<IValidator<CreateModelRequest>, CreateModelRequestValidator>();
            services.AddScoped<IValidator<UpdateModelRequest>, UpdateModelRequestValidator>();
            services.AddScoped<IValidator<ModelTagOperationRequest>, ModelTagOperationRequestValidator>();
            services.AddScoped<IValidator<SearchModelRequest>, SearchModelRequestValidator>();
            
            return services;
        }

        /// <summary>
        /// Registers database context and identity services
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The application configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddDatabaseAndIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            // Add database context
            services.AddDbContext<MyDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Add identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
            })
            .AddEntityFrameworkStores<MyDbContext>()
            .AddDefaultTokenProviders();
            
            return services;
        }

        /// <summary>
        /// Registers repositories
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // User and authentication repositories
            services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            
            // Content repositories
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IConversationPromptResponseRepository, ConversationPromptResponseRepository>();
            services.AddScoped<IPromptRepository, PromptRepository>();
            services.AddScoped<ISystemMessageRepository, SystemMessageRepository>();
            services.AddScoped<IAttachmentRepository, AttachmentRepository>();
            services.AddScoped<IFolderRepository, FolderRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            
            // History repositories
            services.AddScoped<IGetHistoryRepository, GetHistoryRepository>();
            services.AddScoped<ISetHistoryRepository, SetHistoryRepository>();
            
            // Model related repositories
            services.AddScoped<IAIModelRepository, AIModelRepository>();
            services.AddScoped<IAIResponseRepository, AIResponseRepository>();
            services.AddScoped<IModelTagRepository, ModelTagRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            services.AddScoped<IPaginationRepository, PaginationRepository>();

            // Unit of work
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            return services;
        }

        /// <summary>
        /// Registers JWT authentication
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The application configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
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
            
            return services;
        }

        /// <summary>
        /// Configures CORS for the application
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection ConfigureCors(this IServiceCollection services)
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
            
            return services;
        }

        /// <summary>
        /// Configures caching services
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The application configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection ConfigureCache(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));
            
            return services;
        }

        /// <summary>
        /// Configures Swagger documentation
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AdminService", Version = "v1" });

                // Add JWT Authentication support in Swagger
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
            
            return services;
        }

        /// <summary>
        /// Registers health monitoring services
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddHealthMonitoring(this IServiceCollection services)
        {
            // Register HealthController with dependencies injected from other services:
            // - IInferenceEngineConnector from AddInferenceServices
            // - IInferenceEngineConfiguration from Infrastructure.Extensions.ServiceExtensions.AddConfiguration
            services.AddTransient<Controllers.HealthController>();
            
            return services;
        }
    }
}
