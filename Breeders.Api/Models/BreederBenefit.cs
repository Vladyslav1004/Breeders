namespace Breeders.Api.Models;

public class BreederBenefit
{
    public Guid BreederId { get; set; }

    public int FreeLimit { get; set; }

    public int UsedCount { get; set; }
}