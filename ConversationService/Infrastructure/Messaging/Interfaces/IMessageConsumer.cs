using System.Threading;
using System.Threading.Tasks;

namespace ConversationService.Infrastructure.Messaging.Interfaces;

public interface IMessageConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    bool IsConnected { get; }
} 