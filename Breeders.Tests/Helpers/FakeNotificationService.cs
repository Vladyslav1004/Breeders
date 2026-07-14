using Breeders.Api.Services.Interfaces;

namespace Breeders.Tests.Helpers;

public class FakeNotificationService : INotificationService
{
    public bool WasCalled { get; private set; }

    public Guid? LastBreederId { get; private set; }

    public Guid? LastLitterId { get; private set; }

    public Task SendLitterPublishedEmailAsync(
        Guid breederId,
        Guid litterId,
        CancellationToken cancellationToken = default)
    {
        WasCalled = true;
        LastBreederId = breederId;
        LastLitterId = litterId;

        return Task.CompletedTask;
    }
}