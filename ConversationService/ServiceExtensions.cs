﻿using FluentValidation;
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
using Ollama_DB_layer.Repositories.AttachmentRepo;
using Ollama_DB_layer.Repositories.FolderRepo;
using Ollama_DB_layer.Repositories.NoteRepo;
using Ollama_DB_layer.UOW;
using OllamaSharp;
using StackExchange.Redis;
using System.Text;
using ConversationServices.Services.ConversationService;
using ConversationServices.Services.ConversationService.DTOs;
using ConversationServices.Controllers.Validators;
using ConversationServices.Services.ChatService;
using ConversationServices.Services.ChatService.DTOs;
using ConversationServices.Infrastructure.Caching;
using ConversationServices.Services.NoteService;
using ConversationServices.Services.FolderService.DTOs;
using ConversationServices.Services.FeedbackService;
using ConversationServices.Services.FeedbackService.DTOs;
using ConversationServices.Services.FolderService;
using ConversationService.Infrastructure.Integration;
using ConversationService.Infrastructure.Rag.Embedding;
using ConversationService.Infrastructure.Rag.VectorDb;
using ConversationService.Infrastructure.Rag.Options;
using ConversationService.Services.Rag.Interfaces;
using ConversationService.Services.Rag.Implementation;
using ConversationService.Controllers.Document.Validators;
using ConversationService.Infrastructure.Document.Options;
using ConversationService.Infrastructure.Document.Storage;
using ConversationService.Services.Document.Implementation;
using ConversationService.Services.Document.Interfaces;
using ConversationService.Services.Document.Processors.Base;
using ConversationService.Services.Document.Processors.PDF;
using ConversationService.Services.Document.Processors.Text;
using ConversationService.Services.Document.Processors.Word;
using ConversationService.Services.Document.DTOs.Requests;
using ConversationService.Infrastructure.Configuration;
using ConversationService.Infrastructure.Messaging.Extensions;

namespace ConversationServices
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
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add InferenceEngine configuration and messaging
            AddInferenceEngineConfiguration(services, configuration);
            
            // Chat-related services
            services.AddScoped<ChatHistoryManager>();
            services.AddScoped<IChatService, ChatService>();

            // folder service 
            services.AddScoped<IFolderService, FolderService>();
            services.AddScoped<IValidator<CreateFolderRequest>, CreateFolderRequestValidator>();
            services.AddScoped<IValidator<UpdateFolderRequest>, UpdateFolderRequestValidator>();

            // Register ConversationService
            services.AddScoped<IConversationService, Services.ConversationService.ConversationService>();
            // Register NoteService
            services.AddScoped<INoteService, NoteService>();

            // Register FeedbackService
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<IValidator<AddFeedbackRequest>, AddFeedbackRequestValidator>();
            services.AddScoped<IValidator<UpdateFeedbackRequest>, UpdateFeedbackRequestValidator>();

            // Configure RAG Options
            services.Configure<RagOptions>(configuration.GetSection("Rag"));
            services.Configure<PineconeOptions>(configuration.GetSection("Pinecone"));

            // Register RAG Infrastructure
            services.AddSingleton<ITextEmbeddingGeneration, OllamaTextEmbeddingGeneration>(sp =>
            {
                var ragOptions = sp.GetRequiredService<IOptions<RagOptions>>().Value;
                var inferenceConfig = sp.GetRequiredService<IInferenceEngineConfiguration>();
                return new OllamaTextEmbeddingGeneration(ragOptions.OllamaEmbeddingModelId, inferenceConfig);
            });
            services.AddSingleton<IPineconeService, PineconeService>();

            // Register RAG Services
            services.AddScoped<IRagIndexingService, ConversationService.Services.Rag.Implementation.RagIndexingService>();
            services.AddScoped<IRagRetrievalService, ConversationService.Services.Rag.Implementation.RagRetrievalService>();

            // Configure and register Document Services
            ConfigureDocumentServices(services, configuration);

            // Register validators 
            services.AddScoped<IValidator<OpenConversationRequest>, OpenConversationRequestValidator>();
            services.AddScoped<IValidator<UpdateConversationRequest>, UpdateConversationRequestValidator>();

            // Register PromptRequestValidator (keep for backward compatibility)
            services.AddScoped<PromptRequestValidator>();

            // Register ChatRequestValidator for the ChatController
            services.AddScoped<ChatRequestValidator>();
            services.AddScoped<IValidator<PromptRequest>>(provider =>
            {
                // Use ChatRequestValidator for the ChatController
                return provider.GetRequiredService<ChatRequestValidator>();
            });

            // Register HTTP context accessor
            services.AddHttpContextAccessor();
        }

        // Configure InferenceEngine Configuration and RabbitMQ Service Discovery
        private static void AddInferenceEngineConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the URL validator first
            services.AddSingleton<ConversationService.Infrastructure.Messaging.Validators.IUrlValidator, ConversationService.Infrastructure.Messaging.Validators.UrlValidator>();
            
            // Register the InferenceEngine configuration service
            services.AddSingleton<IInferenceEngineConfiguration, InferenceEngineConfiguration>();
            
            // Register the OllamaApiClient using the InferenceEngine configuration
            services.AddScoped<IOllamaApiClient>(sp => 
            {
                var inferenceConfig = sp.GetRequiredService<IInferenceEngineConfiguration>();
                return new OllamaApiClient(inferenceConfig.GetBaseUrl());
            });
            
            // Register the InferenceEngineConnector which will use the new configuration service
            services.AddScoped<IInferenceEngineConnector, InferenceEngineConnector>();
            
            // Register RabbitMQ messaging services for service discovery
            services.AddMessagingServices(configuration);
        }

        // Configure Document Management Services
        private static void ConfigureDocumentServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configure document options
            services.Configure<DocumentManagementOptions>(
                configuration.GetSection("DocumentManagement"));

            // Register document storage
            services.AddSingleton<IDocumentStorage, FileSystemDocumentStorage>();

            // Register document processors
            services.AddSingleton<IDocumentProcessor, PdfDocumentProcessor>();
            services.AddSingleton<IDocumentProcessor, TextDocumentProcessor>();
            services.AddSingleton<IDocumentProcessor, WordDocumentProcessor>();

            // Register document services
            services.AddScoped<IDocumentManagementService, DocumentManagementService>();
            services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();

            // Register document validators
            services.AddScoped<IValidator<UploadDocumentRequest>, DocumentRequestValidator>();
        }



        // Register CORS
        public static void ConfigureCors(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>())
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
        public static void ConfigureSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(c =>
            {
                var version = configuration["Swagger:Version"] ?? "v1";
                var title = configuration["Swagger:Title"] ?? "ConversationService";
                c.SwaggerDoc(version, new OpenApiInfo { Title = title, Version = version });

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
