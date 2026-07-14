namespace Breeders.Api.Exceptions;

public class ForbiddenException : DomainException
{
    public ForbiddenException(string message)
        : base(
            message,
            StatusCodes.Status403Forbidden,
            "forbidden")
    {
    }
}