using Breeders.Api.Enums;

namespace Breeders.Api.DTOs;

public class LitterQueryParameters
{
    public LitterStatus? Status { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}