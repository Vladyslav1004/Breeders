using Breeders.Api.Services.Interfaces;

namespace Breeders.Api.Services;

public class ConsoleNotificationService : INotificationService
{
    private readonly ILogger<ConsoleNotificationService> _logger;

    public ConsoleNotificationService(
        ILogger<ConsoleNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendLitterPublishedEmailAsync(
        Guid breederId,
        Guid litterId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Email notification sent to breeder {BreederId}. " +
            "Litter {LitterId} was successfully published.",
            breederId,
            litterId);

        return Task.CompletedTask;
    }
}