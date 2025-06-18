using Microsoft.Extensions.Configuration;  
using Microsoft.Extensions.DependencyInjection;  
using AdminService.Infrastructure.Caching;  
using AdminService.Infrastructure.Caching.Interfaces;  
using AdminService.Infrastructure.Configuration;  
using AdminService.Infrastructure.Configuration.Options;  
using AdminService.Infrastructure.Validation;  
using AdminService.Infrastructure.Messaging.Resilience;  
using AdminService.Infrastructure.Messaging.Consumers;  
using StackExchange.Redis;
  
namespace AdminService.Infrastructure.Extensions  
{  
    /// <summary>  
    /// Extensions for registering infrastructure services  
    /// </summary>  
    public static class ServiceExtensions  
    {  
        /// <summary>  
        /// Adds caching services to the service collection  
        /// </summary>  
        /// <param name="services">The service collection</param>  
        /// <param name="configuration">The application configuration</param>  
        /// <returns>The service collection</returns>  
        public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)  
        {  
            // Register caching options from configuration
            services.Configure<RedisCacheSettings>(configuration.GetSection("RedisCaching"));
            
            // Register Redis connection multiplexer
            services.AddSingleton<IConnectionMultiplexer>(sp => 
            {
                var connectionString = configuration.GetConnectionString("Redis");
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = "localhost";
                }
                return ConnectionMultiplexer.Connect(connectionString);
            });
            
            // Register Redis cache service
            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            
            // Register cache manager
            services.AddSingleton<ICacheManager, CacheManager>();
            
            return services;  
        } 
  
        /// <summary>  
        /// Adds configuration services to the service collection  
        /// </summary>  
        /// <param name="services">The service collection</param>  
        /// <param name="configuration">The application configuration</param>  
        /// <returns>The service collection</returns>  
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)  
        {  
            // Register options  
            services.Configure<InferenceEngineOptions>(configuration.GetSection("InferenceEngine"));  
            // Register validation  
            services.AddSingleton<IUrlValidator, UrlValidator>();  
            // Register configuration  
            services.AddSingleton<IInferenceEngineConfiguration, InferenceEngineConfiguration>();  
            return services;  
        }  
  
        /// <summary>  
        /// Adds messaging services to the service collection  
        /// </summary>  
        /// <param name="services">The service collection</param>  
        /// <param name="configuration">The application configuration</param>  
        /// <returns>The service collection</returns>  
        public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)  
        {  
            // Register RabbitMQ options from configuration
            services.Configure<RabbitMQOptions>(configuration.GetSection("RabbitMQ"));

            // Register resilience policies for RabbitMQ
            services.AddSingleton<RabbitMQResiliencePolicies>();

            // Register InferenceUrlConsumer as a background service 
            // This will automatically run on application startup and handle 
            // URL update messages from RabbitMQ
            services.AddHostedService<InferenceUrlConsumer>();

            return services;  
        }  
    }  
} 
