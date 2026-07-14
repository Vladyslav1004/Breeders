using Breeders.Api.Models.DTOs;

namespace Breeders.Api.Services.Interfaces;

public interface ILitterService
{
    Task<LitterResponse> PublishAsync(
        Guid litterId,
        Guid breederId,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<LitterResponse>> GetLittersAsync(
        Guid breederId,
        LitterQueryParameters parameters,
        CancellationToken cancellationToken = default);
}