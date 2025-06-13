using ConversationService.Infrastructure.Messaging.Consumers;
using ConversationService.Infrastructure.Messaging.Interfaces;
using ConversationService.Infrastructure.Messaging.Options;
using ConversationService.Infrastructure.Messaging.Resilience;
using ConversationService.Infrastructure.Messaging.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConversationService.Infrastructure.Messaging.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessagingServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add validators
        services.AddSingleton<UrlValidator>();
        
        // Add configuration
        services.Configure<RabbitMQOptions>(options => 
            configuration.GetSection("RabbitMQ").Bind(options));
        
        // Add resilience policies
        services.AddSingleton<RabbitMQResiliencePolicies>();
        
        // Add message consumers (as both the interface and hosted service)
        services.AddSingleton<InferenceUrlConsumer>();
        services.AddSingleton<IMessageConsumer>(provider => provider.GetRequiredService<InferenceUrlConsumer>());
        services.AddHostedService(provider => provider.GetRequiredService<InferenceUrlConsumer>());
        
        return services;
    }
} 