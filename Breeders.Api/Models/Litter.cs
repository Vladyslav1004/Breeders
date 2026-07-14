using Breeders.Api.Enums;

namespace Breeders.Api.Models;

public class Litter
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BreederId { get; set; }

    public LitterStatus Status { get; set; } = LitterStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}