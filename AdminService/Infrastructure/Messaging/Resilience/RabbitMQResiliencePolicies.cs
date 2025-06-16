using System;  
using Microsoft.Extensions.Logging;  
using Polly;  
using Polly.CircuitBreaker;  
  
namespace AdminService.Infrastructure.Messaging.Resilience  
{  
    /// <summary>  
    /// Resilience policies for RabbitMQ operations  
    /// </summary>  
    public class RabbitMQResiliencePolicies  
    {  
        /// <summary>  
        /// Retry policy for handling transient failures  
        /// </summary>  
        public IAsyncPolicy RetryPolicy { get; }  
  
        /// <summary>  
        /// Circuit breaker policy to prevent continuous failures  
        /// </summary>  
        public ICircuitBreakerPolicy CircuitBreakerPolicy { get; }  
  
        /// <summary>  
        /// Constructor for RabbitMQResiliencePolicies  
        /// </summary>  
        /// <param name="logger">Logger</param>  
        public RabbitMQResiliencePolicies(ILogger<RabbitMQResiliencePolicies> logger)  
        {  
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
                        logger.LogError(ex, "Circuit breaker opened for {BreakDelay}s", breakDelay.TotalSeconds);  
                    },  
                    onReset: () =>  
                    {  
                        logger.LogInformation("Circuit breaker reset");  
                    },  
                    onHalfOpen: () =>  
                    {  
                        logger.LogInformation("Circuit breaker half-open");  
                    });  
        }  
    }  
} 
