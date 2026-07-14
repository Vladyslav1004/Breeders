using Breeders.Api.Exceptions;

namespace Breeders.Api.Extensions;

public static class BreederIdHeaderExtensions
{
    public static Guid ParseBreederId(this string? breederIdHeader)
    {
        if (string.IsNullOrWhiteSpace(breederIdHeader))
        {
            throw new UnauthorizedException(
                "X-Breeder-Id header is required.");
        }

        if (!Guid.TryParse(breederIdHeader, out var breederId))
        {
            throw new UnauthorizedException(
                "X-Breeder-Id header must contain a valid GUID.");
        }

        if (breederId == Guid.Empty)
        {
            throw new UnauthorizedException(
                "X-Breeder-Id header cannot contain an empty GUID.");
        }

        return breederId;
    }
}