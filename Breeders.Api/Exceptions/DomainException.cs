namespace Breeders.Api.Exceptions;

public class DomainException : Exception
{
    public int StatusCode { get; }

    public string ErrorCode { get; }

    public DomainException(
        string message,
        int statusCode = StatusCodes.Status400BadRequest,
        string errorCode = "domain_error")
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}