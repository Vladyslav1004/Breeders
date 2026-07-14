namespace Breeders.Api.Services.Interfaces;

public interface INotificationService
{
    Task SendLitterPublishedEmailAsync(
        Guid breederId,
        Guid litterId,
        CancellationToken cancellationToken = default);
}