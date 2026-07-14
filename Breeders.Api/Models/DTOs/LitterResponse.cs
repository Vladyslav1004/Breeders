using Breeders.Api.Enums;

namespace Breeders.Api.Models.DTOs;

public class LitterResponse
{
    public Guid Id { get; set; }

    public LitterStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}