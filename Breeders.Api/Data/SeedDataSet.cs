using Breeders.Api.Models;

namespace Breeders.Api.Data;

public sealed class SeedDataSet
{
    public required Guid PrimaryBreederId { get; init; }

    public required Guid SecondBreederId { get; init; }

    public required Guid ApprovedLitterId { get; init; }

    public required Guid DraftLitterId { get; init; }

    public required Guid SubmittedLitterId { get; init; }

    public required Guid OtherBreederLitterId { get; init; }

    public required IReadOnlyCollection<BreederBenefit>
        BreederBenefits
    { get; init; }

    public required IReadOnlyCollection<Litter>
        Litters
    { get; init; }
}