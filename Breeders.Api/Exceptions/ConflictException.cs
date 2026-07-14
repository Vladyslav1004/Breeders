namespace Breeders.Api.Exceptions;

public class ConflictException : DomainException
{
    public ConflictException(string message)
        : base(
            message,
            StatusCodes.Status409Conflict,
            "conflict")
    {
    }
}