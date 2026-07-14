namespace Breeders.Api.Exceptions;

public class ValidationException : DomainException
{
    public ValidationException(string message)
        : base(
            message,
            StatusCodes.Status400BadRequest,
            "validation_error")
    {
    }
}