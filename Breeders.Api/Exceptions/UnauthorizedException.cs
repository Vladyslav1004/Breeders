namespace Breeders.Api.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message)
        : base(
            message,
            StatusCodes.Status401Unauthorized,
            "unauthorized")
    {
    }
}