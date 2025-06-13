using System;
using Microsoft.Extensions.Logging;
using Polly;

namespace ConversationService.Infrastructure.Messaging.Resilience;

public class RabbitMQResiliencePolicies
{
    private readonly ILogger<RabbitMQResiliencePolicies> _logger;
    public IAsyncPolicy RetryPolicy { get; }
    public IAsyncPolicy CircuitBreakerPolicy { get; }
    private bool _isCircuitOpen;
    
    public bool IsConnected => !_isCircuitOpen;

    public RabbitMQResiliencePolicies(ILogger<RabbitMQResiliencePolicies> logger)
    {
        _logger = logger;
        _isCircuitOpen = false;
        
        RetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(exception, 
                        "Error connecting to RabbitMQ. Retry attempt {RetryCount} after {RetryTimeSpan}s", 
                        retryCount, timeSpan.TotalSeconds);
                });

        CircuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (ex, breakDelay) =>
                {
                    _isCircuitOpen = true;
                    logger.LogError(ex, "Circuit breaker opened for {BreakDelay}s", breakDelay.TotalSeconds);
                },
                onReset: () =>
                {
                    _isCircuitOpen = false;
                    logger.LogInformation("Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Circuit breaker half-open");
                });
    }
} 