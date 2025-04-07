using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AuthenticationService.Helpers;
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
using Ollama_DB_layer.UOW;
using StackExchange.Redis;
using System.Text;
using FluentValidation;
using Ollama_DB_layer.Repositories.RefreshTokenRepo;

namespace AuthenticationService
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
            services.Configure<JWT>(configuration.GetSection("JWT"));
            services.AddScoped<JWTManager>();
            _ = services.AddScoped<IAuthService, AuthService>();

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

            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        // Register Services
        public static void AddApplicationServices(this IServiceCollection services)
        {
            

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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Authentication", Version = "v1" });

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
